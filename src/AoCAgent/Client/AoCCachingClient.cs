using System.Reflection;
using LiteDB;

namespace mazharenko.AoCAgent.Client;

internal class AoCCachingClient(int year, IAoCClient underlyingClient) : IAoCClient
{
	public async Task<string> LoadInput(DayNum day)
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

	public async Task<SubmissionResult> SubmitAnswer(DayNum day, PartNum part, string answer)
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
		{// todo extract?
			var stats = db.GetCollection<DbStats>().Query().FirstOrDefault();
			stats.Solved = 
				stats.Solved.Append(new DbPartId(day, part))
				.Distinct()
				.ToList();
			db.GetCollection<DbStats>().Update(stats);
		}

		return result;
	}

	public async Task AcquireStarLast(DayNum day)
	{
		using var db = ConnectToDb();
		await underlyingClient.AcquireStarLast(day);
		var stats = db.GetCollection<DbStats>().Query().FirstOrDefault();
		stats.Solved =
			stats.Solved.Append(new DbPartId(day, PartNum._2))
				.Distinct()
				.ToList();
		db.GetCollection<DbStats>().Update(stats);
	}
	
	public async Task<Stats> GetDayResults()
	{
		using var db = ConnectToDb();
		var stats = db.GetCollection<DbStats>().Query().FirstOrDefault();
		if (stats is not null && stats.Timestamp > DateTime.Now.AddHours(-2))
			return new Stats(
				stats.Solved
					.Select(x => (DayNum.Create(x.DayNum), PartNum.Create(x.Part)))
					.ToHashSet()
				);

		var actualResults = await underlyingClient.GetDayResults();
		db.GetCollection<DbStats>().Upsert(
			new DbStats
			{
				Solved = actualResults.GetSolved().Select(
					x => new DbPartId(x.day.Num, x.part.Num)
				).ToList(),
				Timestamp = DateTime.Now
			}
		);
		return actualResults;
	}
}