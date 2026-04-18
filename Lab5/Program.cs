/**
runtime env
{
dict
get
set
}

tree interpret
{
eval(expr);
exec(stmt);
}
*/

using Lab1;
using Lab2;
using Lab3;
using Lab4;

namespace Lab5
{
    public class Program
  {
    public static void Main()
    {
      string program = """
      var limit = 10;
      var current = 0;
      var a = "atata";
      var i;

      while (current < limit) {
          if (current == 5) {
              print current * 100;
          } else {
              var c = 5;
              print current;
          }
          current++;
      }

      a = a + a;
      i = "atataatata";
      print a == i;

      print c / 0;
      print c / 10;

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
        
        printer.Print(statements);

        // var analyzer = new SemanticAnalyzer();
        // analyzer.analyze(statements);
        // foreach (var e in analyzer.errnwarn) Console.WriteLine(e);

        var inter = new Interpreter();
        foreach (var s in statements) {
          inter.executeStatement(s);
          // Console.WriteLine(inter.runtimeEnvDump());
        }

      } catch (Exception e)
      {
        Console.WriteLine(e.Message);
      }
    }
  }
}