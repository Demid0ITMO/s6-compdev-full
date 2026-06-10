using Lab1;
using Lab2;
using Lab3;
using Lab5;
using Lab7;

namespace Lab8
{
    public class Program
  {
    public static void Main(string[] args)
    {
      string program = """
      var x = 10;
      var y = 20;
      var sum = x + y;
      print "sum = " + sum;

      if (sum > 25) {
          print "sum > 25";
      } else {
          print "sum <= 25";
      }

      var i = 5;
      while (i > 0) {
          print i;
          i = i - 1;
      }

      array a = [1, 2, 3, 4];
      a[2] = a[1] * 10;
      print "a[2] = " + a[2];

      array words = ["hello", "world"];
      words[1] = words[1] + "!!!";
      print words[0] + " " + words[1];

      fun add(a: double, b: double) {
          return a + b;
      }
      print "add(5,7) = " + add(5, 7);

      fun fact(n: double) {
          if (n <= 1) {
              return 1;
          } else {
              return n * fact(n - 1);
          }
      }
      print "fact(6) = " + fact(6);

      fun func(n: double) {
        if (true) {
          return 1;
        }
      }

      var b = true;
      var c = false;
      if (b && !c) {
          print "b and not c is true";
      }

      if (false) {
        print "C";
      } else {
        print func(52);
      }
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
      var p3 = args.Length > 2 ? args[2] : "dump.txt";
      
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


      analyzer.analyze(statements1);
      foreach (var e in analyzer.errnwarn) Console.WriteLine(e);

      if (!analyzer.errors.Any()) {
        var inter = new Interpreter();
        foreach (var s in statements1) {
          inter.executeStatement(s);
        }
        using (var fileWriter = new StreamWriter(p3)) {
          Console.SetOut(fileWriter);

          Console.WriteLine(inter.runtimeEnvDump());

          fileWriter.Flush();
        }
        Console.SetOut(originalOut);
      }
    }
  }
}