using Lab1;

namespace Lab2 
{
  public class Program
  {
    public static void Main()
    {
      Lexer lexer = new Lexer("""
      var limit = 10;
      var current = 0;

      while (current < limit) {
          if (current == 5) {
              print current * 100;
          } else {
              print current;
          }
          current++;
      }
      """);
      List<Token> tokens = [];
      List<Statement> statements = [];
      try
      {
        tokens = lexer.extract();
        
        foreach (var t in tokens) Console.WriteLine(t.ToString());

        var parser = new Parser(tokens);
        statements = parser.parse();

        var printer = new AstPrinter();
        printer.Print(statements);
      } catch (Exception e)
      {
        Console.WriteLine(e.Message);
      }
    }
  }
}