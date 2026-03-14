using Lab1;

public class Program
{
  public static void Main()
  {
    Lexer lexer = new Lexer("var a = 5; int b = a + 3; if (a == b) a = a - 1;");
    List<Token> tokens = [];

    try
    {
      tokens = lexer.extract();
    } catch (Exception e)
    {
      Console.WriteLine(e.ToString());
    }

    foreach (Token t in tokens)
    {
      Console.WriteLine(t.ToString());
    }
  }
}