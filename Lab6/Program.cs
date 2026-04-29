using Lab1;
using Lab2;
using Lab3;
using Lab5;

namespace Lab6
{
    public class Program
  {
    public static void Main()
    {
      string program = """
      fun add(a: double, tratata:double) {
        return a + 1 + tratata;
      }

      fun minus(a: double, b: double, c: double, d: double) {
        var k = add(a, b) - c * d;
        return k;
      }

      fun onemore() {
        print "come on";
        return;
      }

      fun aboba(first: string, second: string) {
        if (first == "sasat") {
          return "s*sat " + second;
        } 
        return first + " " + second;
      }

      double bb = add(5 + 14, 4 * 9);
      bb = bb - minus(1, 2, 3, 4);
      print aboba("test", "case");
      print aboba("sasat", "america");
      onemore();

      if (bb > 0) { print bb; }

      add(5, 1, 1);
      """;
;;
      List<Token> tokens = [];
      List<Statement> statements = [];
      var printer = new AstPrinter();
      
      Lexer lexer = new Lexer(program);
      tokens = lexer.extract();
      //foreach (var t in tokens) Console.WriteLine(t.ToString());

      var parser = new Parser(tokens);
      statements = parser.parse();
      
      printer.Print(statements);

      var analyzer = new SemanticAnalyzer();
      analyzer.analyze(statements);
      foreach (var e in analyzer.errnwarn) Console.WriteLine(e);

      if (analyzer.errnwarn.Count() == 0) {
        var inter = new Interpreter();
        foreach (var s in statements) {
          inter.executeStatement(s);
        }
      }
    }
  }
}