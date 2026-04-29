using Lab1;
using Lab2;
using Lab3;
using Lab5;

namespace Tests
{
    public class ProgramTests
    {
        // Вспомогательный метод: прогоняет программу через весь пайплайн
        private (string output, IList<string> errors, IList<string> warnings) RunProgram(string source)
        {
            // Лексический анализ
            var lexer = new Lexer(source);
            var tokens = lexer.extract();

            // Парсинг
            var parser = new Parser(tokens);
            var statements = parser.parse();

            // Семантический анализ
            var analyzer = new SemanticAnalyzer();
            analyzer.analyze(statements);
            var errors = analyzer.errors.ToList();
            var warnings = analyzer.warnings.ToList();

            // Перехват вывода в консоль
            var originalOut = Console.Out;
            using var sw = new StringWriter();
            Console.SetOut(sw);

            // Выполнение
            var interpreter = new Interpreter();
            foreach (var stmt in statements) interpreter.executeStatement(stmt);

            Console.SetOut(originalOut);
            string output = sw.ToString().Trim();

            return (output, errors, warnings);
        }

        // 1. Простой вывод чисел, строк, булевых значений
        [Fact]
        public void PrintLiterals()
        {
            var program = @"
                print 42;
                print ""hello"";
                print true;
                print false;
            ";
            var (output, errors, _) = RunProgram(program);
            Assert.Empty(errors);
            Assert.Equal("42\nhello\nTrue\nFalse", output);
        }

        // 2. Арифметика и скобки
        [Fact]
        public void Arithmetic()
        {
            var program = @"
                print (2 + 3) * 4;
                print 10 - 6 / 2;
                print -(5 + 3);
            ";
            var (output, errors, _) = RunProgram(program);
            Assert.Empty(errors);
            Assert.Equal("20\n7\n-8", output);
        }

        // 3. Логические операции и сравнения
        [Fact]
        public void LogicalAndComparison()
        {
            var program = @"
                print true && false;
                print true || false;
                print 5 > 3;
                print 5 <= 5;
                print ""abc"" == ""abc"";
                print ""x"" != ""y"";
                print !true;
            ";
            var (output, errors, _) = RunProgram(program);
            Assert.Empty(errors);
            Assert.Equal("False\nTrue\nTrue\nTrue\nTrue\nTrue\nFalse", output);
        }

        // 4. Переменные с явным типом и var
        [Fact]
        public void VariableDeclarationAndAssignment()
        {
            var program = @"
                double x = 3;
                string s = ""world"";
                boolean flag = true;
                var y = x + 2;
                print y;
                print s;
                print flag;
            ";
            var (output, errors, _) = RunProgram(program);
            Assert.Empty(errors);
            Assert.Equal("5\nworld\nTrue", output);
        }

        // 5. Присваивание и изменение переменной
        [Fact]
        public void ReassignVariable()
        {
            var program = @"
                double a = 100;
                print a;
                a = a / 2;
                print a;
            ";
            var (output, errors, _) = RunProgram(program);
            Assert.Empty(errors);
            Assert.Equal("100\n50", output);
        }

        // 6. Условный оператор if
        [Fact]
        public void IfStatement()
        {
            var program = @"
                if (true) print 1;
                if (false) print 2;
                if (1 > 0) { print 3; }
                if (1 < 0) print 4; else print 5;
            ";
            var (output, errors, _) = RunProgram(program);
            Assert.Empty(errors);
            Assert.Equal("1\n3\n5", output);
        }

        // 7. Цикл while
        [Fact]
        public void WhileLoop()
        {
            var program = @"
                double i = 0;
                while (i < 3) {
                    print i;
                    i = i + 1;
                }
            ";
            var (output, errors, _) = RunProgram(program);
            Assert.Empty(errors);
            Assert.Equal("0\n1\n2", output);
        }

        // 8. Функции: объявление, вызов, возврат значения
        [Fact]
        public void FunctionDeclarationAndCall()
        {
            var program = @"
                fun square(x: double) {
                    return x * x;
                }
                print square(5);
            ";
            var (output, errors, _) = RunProgram(program);
            Assert.Empty(errors);
            Assert.Equal("25", output);
        }

        // 9. Несколько аргументов, возврат void, вызов print
        [Fact]
        public void FunctionWithMultipleArgsAndVoid()
        {
            var program = @"
                fun greet(name: string) {
                    print ""Hello, "" + name;
                }
                greet(""Alice"");
            ";
            var (output, errors, _) = RunProgram(program);
            Assert.Empty(errors);
            Assert.Equal("Hello, Alice", output);
        }

        // 10. Область видимости: переменная внутри блока не видна снаружи
        [Fact]
        public void BlockScope()
        {
            var program = @"
                {
                    double inner = 99;
                    print inner;
                }
                print inner;
            ";
            var (output, errors, _) = RunProgram(program);
            // Ожидаем семантическую ошибку: inner не объявлена во внешней области
            Assert.NotEmpty(errors);
            Assert.Contains("undeclared", errors[0].ToLower());
        }

        // 11. Переопределение переменной в той же области (ошибка)
        [Fact]
        public void DuplicateVariableError()
        {
            var program = @"
                double x = 5;
                double x = 10;
            ";
            var (_, errors, _) = RunProgram(program);
            Assert.NotEmpty(errors);
            Assert.Contains("already", errors[0].ToLower());
        }

        // 11b. Переопределение неинициализированной переменной (already declared)
        [Fact]
        public void DuplicateUninitializedVariableError()
        {
            var program = @"
                double x;
                double x = 10;
            ";
            var (_, errors, _) = RunProgram(program);
            Assert.NotEmpty(errors);
            Assert.Contains("already declared", errors[0].ToLower());
        }

        // 12. Несоответствие типов при инициализации
        [Fact]
        public void TypeMismatchAssignment()
        {
            var program = @"
                double x = ""some string"";
            ";
            var (_, errors, _) = RunProgram(program);
            Assert.NotEmpty(errors);
            Assert.Contains("type", errors[0].ToLower());
        }

        // 12b. Несоответствие типов при присваивании после объявления
        [Fact]
        public void TypeMismatchReassignment()
        {
            var program = @"
                double x = 5;
                x = ""hello"";
            ";
            var (_, errors, _) = RunProgram(program);
            Assert.NotEmpty(errors);
            Assert.Contains("has type", errors[0].ToLower());
            Assert.Contains("but assigned expression has type", errors[0].ToLower());
        }

        // 13. Необъявленная переменная при использовании
        [Fact]
        public void UndeclaredVariable()
        {
            var program = @"
                print unknown;
            ";
            var (_, errors, _) = RunProgram(program);
            Assert.NotEmpty(errors);
            Assert.Contains("undeclared", errors[0].ToLower());
        }

        // 13b. Использование неинициализированной переменной
        [Fact]
        public void UninitializedVariable()
        {
            var program = @"
                double x;
                print x;
            ";
            var (_, errors, _) = RunProgram(program);
            Assert.NotEmpty(errors);
            Assert.Contains("undefined", errors[0].ToLower());
        }

        // 14. Предупреждение о неиспользуемой инициализированной переменной
        [Fact]
        public void UnusedVariableWarning()
        {
            var program = @"
                double ghost = 42;
                print 1;
            ";
            var (_, _, warnings) = RunProgram(program);
            Assert.NotEmpty(warnings);
            Assert.Contains("defined, but not used", warnings[0].ToLower());
        }

        // 14b. Предупреждение о неиспользуемой объявленной (но не инициализированной) переменной
        [Fact]
        public void UnusedDeclaredVariableWarning()
        {
            var program = @"
                double ghost;
                print 1;
            ";
            var (_, _, warnings) = RunProgram(program);
            Assert.NotEmpty(warnings);
            Assert.Contains("declared, but not used", warnings[0].ToLower());
        }

        // 15. Вызов неопределённой функции (семантическая ошибка)
        [Fact]
        public void UndefinedFunctionCall()
        {
            var program = @"
                print doesNotExist();
            ";
            var (_, errors, _) = RunProgram(program);
            Assert.NotEmpty(errors);
            Assert.Contains("not declared", errors[0].ToLower());
        }

        // 15b. Дублирование функции
        [Fact]
        public void DuplicateFunctionError()
        {
            var program = @"
                fun f() {}
                fun f() {}
            ";
            var (_, errors, _) = RunProgram(program);
            Assert.NotEmpty(errors);
            Assert.Contains("already defined", errors[0].ToLower());
        }

        // 16. Неверное количество аргументов при вызове (ошибка времени выполнения)
        [Fact]
        public void WrongArgumentCountRuntime()
        {
            var program = @"
                fun add(a: double, b: double) {
                    return a + b;
                }
                print add(5);
            ";
            var (output, _, _) = RunProgram(program);
            Assert.Contains("RUNTIME ERROR", output);
        }

        // 17. Рекурсивная функция (факториал)
        [Fact]
        public void RecursiveFactorial()
        {
            var program = @"
                fun fact(n: double) {
                    if (n <= 1) return 1;
                    return n * fact(n - 1);
                }
                print fact(5);
            ";
            var (output, errors, _) = RunProgram(program);
            Assert.Empty(errors);
            Assert.Equal("120", output);
        }

        // 18. Инкремент и декремент (i++ и i--)
        [Fact]
        public void IncrementDecrement()
        {
            var program = @"
                double i = 10;
                print i;
                i++;
                print i;
                i--;
                print i;
            ";
            var (output, errors, _) = RunProgram(program);
            Assert.Empty(errors);
            Assert.Equal("10\n11\n10", output);
        }

        // 19. Строковые операции конкатенации и сравнения
        [Fact]
        public void StringOperations()
        {
            var program = @"
                print ""abc"" + ""def"";
                print ""abc"" == ""abc"";
                print ""abc"" != ""def"";
            ";
            var (output, errors, _) = RunProgram(program);
            Assert.Empty(errors);
            Assert.Equal("abcdef\nTrue\nTrue", output);
        }

        // 20. Булевы операции (&&, ||, !) и их приоритет
        [Fact]
        public void BooleanExpressions()
        {
            var program = @"
                boolean a = true;
                boolean b = false;
                print a && b;
                print a || b;
                print !a;
                print (a || b) && !b;
            ";
            var (output, errors, _) = RunProgram(program);
            Assert.Empty(errors);
            Assert.Equal("False\nTrue\nFalse\nTrue", output);
        }

        // 21. Смешанные типы в бинарных операциях (ошибка типа)
        [Fact]
        public void MixedTypeBinaryError()
        {
            var program = @"
                print 5 + ""text"";
            ";
            var (_, errors, _) = RunProgram(program);
            Assert.NotEmpty(errors);
            Assert.Contains("Unsupported operation", errors[0]);
        }

        // 21b. Недопустимая операция над одинаковыми типами (true + true)
        [Fact]
        public void InvalidBoolAdditionError()
        {
            var program = @"
                print true + true;
            ";
            var (_, errors, _) = RunProgram(program);
            Assert.NotEmpty(errors);
            Assert.Contains("Unsupported operation", errors[0]);
        }

        // 21c. Недопустимая операция вычитания строк
        [Fact]
        public void InvalidStringSubtractionError()
        {
            var program = @"
                print ""a"" - ""b"";
            ";
            var (_, errors, _) = RunProgram(program);
            Assert.NotEmpty(errors);
            Assert.Contains("Unsupported operation", errors[0]);
        }

        // 21d. Недопустимое сравнение строк с >
        [Fact]
        public void InvalidStringComparisonError()
        {
            var program = @"
                print ""a"" > ""b"";
            ";
            var (_, errors, _) = RunProgram(program);
            Assert.NotEmpty(errors);
            Assert.Contains("Unsupported operation", errors[0]);
        }

        // 22. Возврат void из функции и вызов в выражении (семантическая ошибка)
        [Fact]
        public void UseVoidFunctionInExpression()
        {
            var program = @"
                fun nop() {
                    return;
                }
                print nop() + 1;
            ";
            var (_, errors, _) = RunProgram(program);
            Assert.NotEmpty(errors);
            Assert.Contains("operation", errors[0].ToLower());
        }

        // 23. Множественные объявления и использование в разных блоках (shadowing)
        [Fact]
        public void ShadowingAndScope()
        {
            var program = @"
                double x = 10;
                {
                    double x = 20;
                    print x;
                }
                print x;
            ";
            var (output, errors, _) = RunProgram(program);
            Assert.Collection(errors, e => Assert.Contains("Var 'x' already defined", e));
        }

        // 24. Ошибка условия в if (не boolean)
        [Fact]
        public void IfConditionMustBeBoolean()
        {
            var program = @"
                if (4) { print 1; }
            ";
            var (_, errors, _) = RunProgram(program);

            Assert.NotEmpty(errors);
            Assert.Contains("Condition must be boolean", errors[0]);
        }

        // 25. Ошибка условия в while (не boolean)
        [Fact]
        public void WhileConditionMustBeBoolean()
        {
            var program = @"
                while (""loop"") { print 1; }
            ";
            var (_, errors, _) = RunProgram(program);
            Assert.NotEmpty(errors);
            Assert.Contains("Condition must be boolean", errors[0]);
        }

        // 26. Ошибка типа из-за недопустимой унарной операции (-true)
        [Fact]
        public void InvalidUnaryMinusOnBool()
        {
            var program = @"
                print -true;
            ";
            var (_, errors, _) = RunProgram(program);
            Assert.NotEmpty(errors);
            Assert.Contains("Unsupported operation", errors[0]);
        }

        // 26b. Ошибка типа из-за !числа
        [Fact]
        public void InvalidUnaryNotOnNumber()
        {
            var program = @"
                print !5;
            ";
            var (_, errors, _) = RunProgram(program);
            Assert.NotEmpty(errors);
            Assert.Contains("Unsupported operation", errors[0]);
        }

        // 27. Предупреждение о невозможности вывести тип выражения (два неизвестных операнда)
        [Fact]
        public void CannotSolveExpressionTypeWarning()
        {
            var program = @"
                var a;
                var b;
                print a + b;
            ";
            var (_, _, warnings) = RunProgram(program);
            Assert.NotEmpty(warnings);
            Assert.Contains("Can not solve expression type", warnings[0]);
        }
    }
}