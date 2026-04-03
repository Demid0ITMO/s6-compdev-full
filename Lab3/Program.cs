using Lab1;
using Lab2;

namespace Lab3 
{
  public class Program
  {
    public static void Main()
    {
      var lexer = new Lexer("""
      var limit = 10;
      var current = 0;
      var i;

      while (current < limit) {
          if (current == 5) {
              print current * 100;
              print i;
              j = 0;
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

        var analyzer = new SemanticAnalyzer();
        analyzer.analyze(statements);

        foreach (var e in analyzer.errors) Console.WriteLine(e);
      } catch (Exception e)
      {
        Console.WriteLine(e);
      }
    }
  }
}