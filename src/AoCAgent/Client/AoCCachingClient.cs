using System.Collections.Immutable;
using System.Reflection;
using LiteDB;

namespace mazharenko.AoCAgent.Client;

internal class AoCCachingClient : IAoCClient
{
	private readonly int year;
	private readonly IAoCClient underlyingClient;

	public AoCCachingClient(int year, IAoCClient underlyingClient)
	{
		this.year = year;
		this.underlyingClient = underlyingClient;
	}
	
	public async Task<string> LoadInput(Day day)
	{
		using var db = ConnectToDb();
		var inputs = db.GetCollection<DbPuzzleInput>()!;
		if (inputs.FindById((int)day) is { Input: var foundInput })
			return foundInput;
		var inputFromAoc = await underlyingClient.LoadInput(day);
		inputs.Upsert(new DbPuzzleInput { Day = day, Input = inputFromAoc });
		return inputFromAoc;
	}

	private LiteDatabase ConnectToDb()
	{
		var directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
		var dbName = $"{year}.{ThisAssembly.Info.Version}.aoc.db";
		var dbPath = directory is null ? dbName : Path.Combine(directory, dbName);
		var db =  new LiteDatabase($"Filename={dbPath};Collation=en-US/None");
		db.Mapper.TrimWhitespace = false;
		db.Mapper.EmptyStringToNull = false;
		db.GetCollection<DbAttempt>().EnsureIndex(attempt => attempt.PartId);
		return db;
	}

	public async Task<SubmissionResult> SubmitAnswer(Day day, Part part, string answer)
	{
		using var db = ConnectToDb();
		var existingAttempt = db.GetCollection<DbAttempt>().FindOne(attempt =>
			attempt.PartId.DayNum == day.Num
			&& attempt.PartId.Part == part.Num
			&& attempt.Answer == answer
		);
		SubmissionResult result;
		if (existingAttempt is not null)
			result = existingAttempt.Verdict switch
			{
				DbAttemptVerdict.Correct => new SubmissionResult.Correct(),
				DbAttemptVerdict.TooLow => new SubmissionResult.TooLow(),
				DbAttemptVerdict.TooHigh => new SubmissionResult.TooHigh(),
				DbAttemptVerdict.Incorrect => new SubmissionResult.Incorrect()
			};
		else
		{
			result = await underlyingClient.SubmitAnswer(day, part, answer);

			if (result is not SubmissionResult.TooRecently)
				db.GetCollection<DbAttempt>().Upsert(new DbAttempt
				{
					PartId = new DbPartId(day, part),
					Answer = answer,
					Verdict = result switch
					{
						SubmissionResult.Correct => DbAttemptVerdict.Correct,
						SubmissionResult.Incorrect => DbAttemptVerdict.Incorrect,
						SubmissionResult.TooHigh => DbAttemptVerdict.TooHigh,
						SubmissionResult.TooLow => DbAttemptVerdict.TooLow,
						_ => throw new ArgumentOutOfRangeException(nameof(result))
					}
				});
		}

		if (result is SubmissionResult.Correct)
		{
			var stats = db.GetCollection<DbStats>().Query().FirstOrDefault();
			stats.Stats = 
				stats.Stats.Prepend(new DbDayPartStat { Id = new DbPartId(day, part), Solved = true })
				.DistinctBy(stat => stat.Id)
				.ToList();
			db.GetCollection<DbStats>().Update(stats);
		}

		return result;
	}

	public async Task<IImmutableDictionary<(Day, Part), bool>> GetDayResults()
	{
		using var db = ConnectToDb();
		var stats = db.GetCollection<DbStats>().Query().FirstOrDefault();
		if (stats is not null && stats.Timestamp > DateTime.Now.AddHours(-2))
			return stats.Stats.ToDictionary(
				x => (Day.Create(x.Id.DayNum), Part.Create(x.Id.Part)),
				x => x.Solved
			).ToImmutableDictionary();

		var actualResults =
			await underlyingClient.GetDayResults();
		db.GetCollection<DbStats>().Upsert(
			new DbStats
			{
				Stats = actualResults.Select(
					x => new DbDayPartStat{Id = new DbPartId(x.Key.Item1.Num, x.Key.Item2.Num), Solved = x.Value}
				).ToList(),
				Timestamp = DateTime.Now
			}
		);
		return actualResults;
	}

	public void Dispose()
	{
		underlyingClient.Dispose();
	}
}