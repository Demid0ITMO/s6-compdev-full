using Lab1;
using Lab2;
using Lab3;
using Lab5;

namespace Lab7
{
    public class Program
  {
    public static void Main(string[] args)
    {
      string program = """
      double a = 2 + 3 * 4;
      double b = (10 - 5) / 2;
      var c = 5 > 3 && 2 < 1;
      var d = !true || false;
      string e = "Hello, " + "world";

      double x = 42;
      double y = x + 0;
      double z = x * 1;
      double w = x * 0;
      var t = true && (x > 10);
      var f = false || (x < 30);

      fun testReturn() {
          print "start";
          return 123;
          print "dead";
          var deadVar = 999;
      }

      if (true) {
          print "always printed";
      } else {
          print "never printed";
      }

      if (false) {
        print "never printed";
      }

      while (false) {
          print "infinite loop?";
      }

      var flipped = !!true;
      double negneg = -(-42);

      fun constCalc() {
          return 10 * (2 + 3) - 5;
      }

      double result = constCalc();
      print result;

      var unused = 100;
      var unusedNoInit;

      print "Optimization test finished";
      """;

      List<Token> tokens = [];
      List<Statement> statements = [];
      var printer = new AstPrinter();
      var optimizer = new Optimizer();
      var analyzer = new SemanticAnalyzer();

      
      var lexer = new Lexer(program);
      tokens = lexer.extract();

      var parser = new Parser(tokens);
      statements = parser.parse();
      
      
      var originalOut = Console.Out;
      var p1 = args.Length > 0 ? args[0] : "before.txt";
      var p2 = args.Length > 1 ? args[1] : "after.txt";
      
      using (var fileWriter = new StreamWriter(p1)) {
        Console.SetOut(fileWriter);

        printer.Print(statements);

        fileWriter.Flush();
      }
      Console.SetOut(originalOut);
      
      var statements1 = optimizer.optimize(statements);

      using (var fileWriter = new StreamWriter(p2)) {
        Console.SetOut(fileWriter);

        printer.Print(statements1);

        fileWriter.Flush();
      }
      Console.SetOut(originalOut);


      analyzer.analyze(statements);
      foreach (var e in analyzer.errnwarn) Console.WriteLine(e);

      if (!analyzer.errors.Any()) {
        var inter = new Interpreter();
        foreach (var s in statements) {
          inter.executeStatement(s);
        }
      }
    }
  }
}