using Lab1;
using Lab2;
using Lab3;

namespace Lab4
{
  public class Program
  {
    public static void Main()
    {
      string program = """
      var limit = 10;
      var current = 0;
      var a = "atata";
      var i; var b;

      while (current < limit) {
          if (current == 5) {
              print current * 100;
          } else {
              var c = 5;
              print current;
          }
          current++;
      }

      c++;
      print i;
      i = a + current;
      current / 5;
      i = a; i = current;
      var b = "a";
      !b; -limit;
      """;
      List<Token> tokens = [];
      List<Statement> statements = [];
      var printer = new AstPrinter();
      try
      {
        Lexer lexer = new Lexer(program);
        tokens = lexer.extract();
        //foreach (var t in tokens) Console.WriteLine(t.ToString());

        var parser = new Parser(tokens);
        statements = parser.parse();
        
        // printer.Print(statements);

        var analyzer = new SemanticAnalyzer();
        analyzer.analyze(statements);
        foreach (var e in analyzer.errnwarn) Console.WriteLine(e);

      } catch (Exception e)
      {
        Console.WriteLine(e.Message);
      }
    }
  }
}