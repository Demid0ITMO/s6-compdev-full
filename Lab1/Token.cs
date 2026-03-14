namespace Lab1
{
  public class Token(TokenType TokType, string Value, int Position, int Row, int Column)
  {
    public TokenType TokType { get; } = TokType;
    public string Value { get; } = Value;
    public int Position { get; } = Position;
    public int Row { get; } = Row;
    public int Column { get; } = Column;


    public override string ToString()
    {
      return $"[{Row}:{Column}] Token(\"{Value}\", {TokType})";
    }
  }
}