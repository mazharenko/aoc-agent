namespace mazharenko.AoCAgent.Client;

internal abstract record SubmissionResult
{
	private SubmissionResult(){}
	public record Correct : SubmissionResult;
	public record Incorrect : SubmissionResult;
	public record TooLow : SubmissionResult;
	public record TooHigh : SubmissionResult;
	public record TooRecently(TimeSpan LeftToWait) : SubmissionResult;
}

internal class Stats(ISet<(DayNum, PartNum)> solvedParts)
{
	public Stats() : this(new HashSet<(DayNum, PartNum)>())
	{
	}

	public bool IsSolved(DayNum day, PartNum part)
	{
		return solvedParts.Contains((day, part));
	}

	public Stats Solved(DayNum day, PartNum part)
	{
		solvedParts.Add((day, part));
		return this;
	}

	public void Add(DayNum day, PartNum part, bool solved)
	{
		if (solved)
			Solved(day, part);
		else 
			NotSolved(day, part);
	}

	public Stats NotSolved(DayNum day, PartNum part)
	{
		solvedParts.Remove((day, part));
		return this;
	}

	public IEnumerable<(DayNum day, PartNum part)> GetSolved() => solvedParts;

	public int Stars => solvedParts.Count;
}

internal interface IAoCClient 
{
	Task<string> LoadInput(DayNum day);
	Task<SubmissionResult> SubmitAnswer(DayNum day, PartNum part, string answer);
	Task<Stats> GetDayResults();
	Task AcquireStarLast(DayNum day);
}