using Lab1;
using Lab2;

namespace Lab3
{
  public class ReturnException: Exception
  {
    public object? value;
  }
  public class SemanticAnalyzer
  {
    readonly Dictionary<TokenType, DataType> typeCheck = new()
    {
        { TokenType.BOOLTYPE, DataType.BOOL },
        { TokenType.NUMTYPE, DataType.NUM },
        { TokenType.STRTYPE, DataType.STR }
    };
    private class SemanticErr(string message, int row, int column)
    {
        public string msg { get; } = message;
        public int r { get; } = row;
        public int c { get; } = column;
    };
    private SemanticEnv env = new();
    private readonly List<SemanticErr> errs = [];
    private readonly List<SemanticErr> warns = [];
    public IEnumerable<string> errors => errs
        .OrderBy(err => err.r)
        .ThenBy(err => err.c)
        .Select(err => $"Semantic Error [{err.r}:{err.c}]: {err.msg}");
    public IEnumerable<string> warnings => warns
        .OrderBy(err => err.r)
        .ThenBy(err => err.c)
        .Select(err => $"Semantic Warning [{err.r}:{err.c}]: {err.msg}");

    public IEnumerable<string> errnwarn => errors
    .Zip(errs, (a, b) => new SemanticErr(a, b.r, b.c))
    .Concat(warnings
        .Zip(warns, (a, b) => new SemanticErr(a, b.r, b.c))
    )
    .DistinctBy(err => $"{err.r}:{err.c}")
    .OrderBy(e => e.r)
    .ThenBy(e => e.c)
    .Select(e => e.msg);

    public void analyze(IEnumerable<Statement> statements)
    {
      foreach (var statement in statements) {
        var t = visitStmt(statement);
        if (t == DataType.UNKNOWN) errs.Add(new SemanticErr("Unknown type", statement.row, statement.column));
      }
      checkUnusedVariables(1, 1);
    }

    public DataType visitStmt(Statement statement)
    {
        switch (statement)
        {
            case VarStatement v: analyzeVarStatement(v); return DataType.VOID;
            case PrintStatement p: analyzePrintStatement(p); return DataType.VOID;
            case ExpressionStatement e: return analyzeExpressionStatement(e);
            case BlockStatement b: analyzeBlockStatement(b); return DataType.VOID;
            case IfStatement i: analyzeIfStatement(i); return DataType.VOID;
            case WhileStatement w: analyzeWhileStatement(w); return DataType.VOID;
            case FuncDeclarationStatement f: analyzeFuncDeclarationStatement(f); return DataType.VOID;
            case ReturnStatement r: analyzeReturnStatement(r); return DataType.VOID;
            default:
                errs.Add(new SemanticErr($"Unsupported statement {statement.GetType().Name}", statement.row, statement.column));
                return DataType.VOID;
        }
    }

    public DataType visitExpr(Expression expression, int r, int c)
    {
        switch (expression)
        {
            case NumberExpression n: return DataType.NUM;
            case StringExpression s: return DataType.STR;
            case BooleanExpression b: return DataType.BOOL;
            case VariableExpression v: return analyzeVariableExpression(v, r, c);
            case AssignExpression a: return analyzeAssignExpression(a, r, c);
            case BinaryExpression b: return analyzeBinaryExpression(b, r, c);
            case UnaryExpression u: return analyzeUnaryExpression(u, r, c);
            case FuncCallExpression f: return analyzeFuncCallExpression(f, r, c);
            default:
                errs.Add(new SemanticErr($"Unsupported expression {expression.GetType().Name}", r, c));
                return DataType.VOID;
        }
    }
    
    private void analyzeFuncDeclarationStatement(FuncDeclarationStatement stmt)
    {
        if(!env.defineVar(stmt.name, false, null)) errs.Add(new SemanticErr($"Function '{stmt.name}' already defined", stmt.row, stmt.column));
        else
        {   
            var prevEnv = env;
            env = new SemanticEnv(prevEnv);
            foreach(var arg in stmt.args) {
                typeCheck.TryGetValue(arg.type, out DataType type);
                env.defineVar(arg.name, true, type);
            }
            DataType ret = DataType.VOID;
            try {
                visitStmt(stmt.body);
            } catch (ReturnException e)
            {
                if (e.value != null) ret = (DataType)e.value;
            }
            env = prevEnv;
            env.setInited(stmt.name, ret);
        }
    }

    private DataType analyzeReturnStatement(ReturnStatement stmt)
    {
        var e = new ReturnException();
        e.value = DataType.VOID;
        if (stmt.expr != null) e.value = visitExpr(stmt.expr, stmt.row, stmt.column);
        throw e;
    }

    private void analyzeVarStatement(VarStatement stmt)
    {
        if (!env.defineVar(stmt.name, false, stmt.postype == TokenType.VAR ? null : typeCheck[stmt.postype]))
        {
            if (env.getVar(stmt.name)?.isInited ?? false) errs.Add(new SemanticErr($"Var '{stmt.name}' already defined", stmt.row, stmt.column));
            else errs.Add(new SemanticErr($"Var '{stmt.name}' already declared", stmt.row, stmt.column));
        }
        else if (stmt.initializer != null)
        {
            var type = visitExpr(stmt.initializer, stmt.row, stmt.column);
            if (stmt.postype == TokenType.VAR) env.setInited(stmt.name, type);
            else if (typeCheck[stmt.postype] == type) env.setInited(stmt.name, type);
            else errs.Add(new SemanticErr($"Var must be {typeCheck[stmt.postype]}, but assigned value has type {type}", stmt.row, stmt.column));
        }
    }

    private void analyzePrintStatement(PrintStatement stmt)
    {
        visitExpr(stmt.expression, stmt.row, stmt.column);
    }

    private DataType analyzeExpressionStatement(ExpressionStatement stmt)
    {
        return visitExpr(stmt.expression, stmt.row, stmt.column);
    }

    private void analyzeBlockStatement(BlockStatement stmt)
    {
        var prevEnv = env;
        env = new SemanticEnv(prevEnv);

        foreach (var innerStatement in stmt.statements)
        {
            var t = visitStmt(innerStatement);
            if (t == DataType.UNKNOWN) errs.Add(new SemanticErr("Unknown type", innerStatement.row, innerStatement.column));
        }

        checkUnusedVariables(stmt.row, stmt.column);
        env = prevEnv;
    }

    private void analyzeIfStatement(IfStatement stmt)
    {
        var t = visitExpr(stmt.condition, stmt.row, stmt.column);
        var t1 = visitStmt(stmt.thenBranch);
        DataType t2 = DataType.VOID;

        if (stmt.elseBranch != null)
        {
            t2 = visitStmt(stmt.elseBranch);
        }

        if (t != DataType.BOOL) errs.Add(new SemanticErr("Condition must be boolean", stmt.row, stmt.column));
    }

    private void analyzeWhileStatement(WhileStatement stmt)
    {
        var t = visitExpr(stmt.condition, stmt.row, stmt.column);
        var t1 = visitStmt(stmt.body);

        if (t != DataType.BOOL) errs.Add(new SemanticErr("Condition must be boolean", stmt.row, stmt.column));
    }

    private void checkUnusedVariables(int r, int c)
    {
        foreach (var symbol in env.GetLocalVariables())
        {
            if (!symbol.isUsed)
            {
                if (symbol.isInited) warns.Add(new SemanticErr($"Var '{symbol.name}' is defined, but not used", r, c));
                else warns.Add(new SemanticErr($"Var '{symbol.name}' is declared, but not used", r, c));
            }
        }
    }

    private DataType analyzeFuncCallExpression(FuncCallExpression expr, int r, int c)
    {
        var func = env.getVar(expr.name);

        if (!env.isVarDefined(expr.name) || func == null) {
            errs.Add(new SemanticErr($"Function '{expr.name}' is not declared", r, c));
            return DataType.UNKNOWN;
        }
        
        func.isUsed = true;
        foreach(var arg in expr.args) visitExpr(arg, r, c);
        return func.type;
    }

    private DataType analyzeVariableExpression(VariableExpression expr, int r, int c)
    {
        var symbol = env.getVar(expr.name);
        if (symbol == null)
        {
            errs.Add(new SemanticErr($"Var '{expr.name}' is undeclared", r, c));
        }
        else
        {
            symbol.isUsed = true;

            if (!symbol.isInited)
            {
                errs.Add(new SemanticErr($"Var '{expr.name}' is undefined", r, c));
            }
        }
        return symbol?.type ?? DataType.UNKNOWN;
    }

    private DataType analyzeAssignExpression(AssignExpression expr, int r, int c)
    {
        DataType type = visitExpr(expr.value, r, c);

        if (!env.isVarDefined(expr.name))
        {
            errs.Add(new SemanticErr($"Var '{expr.name}' is undeclared", r, c));
        }
        else
        {
            var v = env.getVar(expr.name);
            if ((v?.isInited ?? false) && (type != v.type)) errs.Add(new SemanticErr($"Var '{expr.name}' has type '{v.type}', but assigned expression has type '{type}'", r, c));
            else if (type != DataType.UNKNOWN) env.setInited(expr.name, type);
        }

        return DataType.VOID;
    }

    private DataType analyzeBinaryExpression(BinaryExpression expr, int r, int c)
    {
        var t1 = visitExpr(expr.first, r, c);
        var t2 = visitExpr(expr.second, r, c);
        List<TokenType> nums_to_num_opers = [
            TokenType.PLUS, 
            TokenType.MINUS, 
            TokenType.MUL, 
            TokenType.DIV,
        ];

        List<TokenType> nums_to_bool_opers = [
            TokenType.EQEQ,
            TokenType.NONEQ,
            TokenType.LT,
            TokenType.RT,
            TokenType.LTEQ,
            TokenType.RTEQ
        ];
        
        List<TokenType> strs_to_str_opers = [
            TokenType.PLUS,
        ];

        List<TokenType> strs_to_bool_opers = [
            TokenType.EQEQ,
            TokenType.NONEQ,
        ];

        List<TokenType> bools_to_bool_opers = [
            TokenType.AND,
            TokenType.OR,
            TokenType.EQEQ,
            TokenType.NONEQ
        ];

        if (t1 != DataType.UNKNOWN && t2 != DataType.UNKNOWN) {
            if (t1 != t2) {
                errs.Add(new SemanticErr($"Unsupported operation {expr.oper} between '{t1}' and '{t2}'", r, c));
                return DataType.UNKNOWN;
            }

            if (t1 == DataType.NUM && nums_to_num_opers.Contains(expr.oper)) return DataType.NUM;
            if (t1 == DataType.NUM && nums_to_bool_opers.Contains(expr.oper)) return DataType.BOOL;
            if (t1 == DataType.STR && strs_to_str_opers.Contains(expr.oper)) return DataType.STR;
            if (t1 == DataType.STR && strs_to_bool_opers.Contains(expr.oper)) return DataType.BOOL;     
            if (t1 == DataType.BOOL && bools_to_bool_opers.Contains(expr.oper)) return DataType.BOOL;

            errs.Add(new SemanticErr($"Unsupported operation between '{t1}' and '{t2}'", r, c));
            return DataType.UNKNOWN;
        } else {
            var T1 = (t1 == DataType.UNKNOWN) ? ((t2 == DataType.UNKNOWN) ? DataType.UNKNOWN : t2) : t1;
            if (T1 == DataType.UNKNOWN)
            {
                int num = 0, str = 0, bol = 0;
                if (nums_to_num_opers.Contains(expr.oper)) num = 1;
                if (nums_to_bool_opers.Contains(expr.oper)) bol = 1;
                if (strs_to_str_opers.Contains(expr.oper)) str = 1;
                if (strs_to_bool_opers.Contains(expr.oper)) bol = 1;
                if (bools_to_bool_opers.Contains(expr.oper)) bol = 1;

                if (num + str + bol == 1)
                {
                    if (num == 1) return DataType.NUM;
                    if (str == 1) return DataType.STR;
                    if (bol == 1) return DataType.BOOL;
                }

                var message = "Can not solve expression type. Possible types are: ";
                if (num != 0) message += $"'{DataType.NUM}'";
                if (str != 0) {
                    if (num != 0) message += ", ";
                    message += $"'{DataType.STR}'";
                }
                if (bol != 0) {
                    if (str != 0) message += ", ";
                    else if (num != 0) message += ", ";
                    message += $"'{DataType.BOOL}'";
                }
                warns.Add(new SemanticErr(message, r, c));
                return DataType.UNKNOWN;
            } 
            switch(T1)
            {
                case DataType.NUM:
                    if (nums_to_num_opers.Contains(expr.oper)) return DataType.NUM;
                    if (nums_to_bool_opers.Contains(expr.oper)) return DataType.BOOL;
                    errs.Add(new SemanticErr($"Unsupported operation between '{t1}' and '{t2}'", r, c));
                    return DataType.UNKNOWN;
                case DataType.STR:
                    if (strs_to_str_opers.Contains(expr.oper)) return DataType.STR;
                    if (strs_to_bool_opers.Contains(expr.oper)) return DataType.BOOL;
                    errs.Add(new SemanticErr($"Unsupported operation between '{t1}' and '{t2}'", r, c));
                    return DataType.UNKNOWN;
                case DataType.BOOL:
                    if (bools_to_bool_opers.Contains(expr.oper)) return DataType.BOOL;
                    errs.Add(new SemanticErr($"Unsupported operation between '{t1}' and '{t2}'", r, c));
                    return DataType.UNKNOWN;
                default:
                    errs.Add(new SemanticErr($"Unsupported operation between '{t1}' and '{t2}'", r, c));
                    return DataType.UNKNOWN;
            }
        }
    }

    private DataType analyzeUnaryExpression(UnaryExpression expr, int r, int c)
    {
        var t = visitExpr(expr.value, r, c);
        if (expr.oper == TokenType.MINUS && t == DataType.NUM) return DataType.NUM;
        if (expr.oper == TokenType.NON && t == DataType.BOOL) return DataType.BOOL;
        errs.Add(new SemanticErr($"Unsupported operation {expr.oper} with {t}", r, c));
        return DataType.UNKNOWN;
    }
  }
}