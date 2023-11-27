using LiteDB;

namespace mazharenko.AoCAgent.Client;
internal record DbPuzzleInput
{
	[BsonId]
	public required int Day { get; init; }
	public required string Input { get; init; } = null!;
}

internal record DbPartId(int DayNum, int Part);

internal record DbDayPartStat
{
	public required DbPartId Id { get; init; }
	public required bool Solved { get; set; }
}

internal record DbStats
{
	[BsonId]
	public int Id => 1;

	public required IList<DbDayPartStat> Stats { get; set; }
	public required DateTime Timestamp { get; set; }
}

internal enum DbAttemptVerdict
{
	Correct, TooLow, TooHigh, Incorrect
}

internal record DbAttempt
{
	public required DbPartId PartId { get; init; }
	public required string Answer { get; init; }
	public required DbAttemptVerdict Verdict { get; init; }
}

