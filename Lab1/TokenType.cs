namespace Lab1
{

  public enum TokenType
  {
      NUMBER, STRING, ID, VAR, 
      PRINT, IF, ELSE, WHILE,
      
      PLUS, MINUS, MUL, DIV, // + - * /
      INCR, DECR, // ++ --
      EQ, EQEQ, NON, NONEQ, // = == ! !=
      LT, RT, LTEQ, RTEQ, // > < >= <=
      AND, OR, // && ||

      LBR, RBR, // ()
      LFBR, RFBR, // {}
      SEMICOLON, // ;

      TBOOL, FBOOL
  }
}