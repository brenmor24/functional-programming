using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class LispishParser
{
    public enum Symbol
    {
        Program,
        SExpr,
        List,
        Seq,
        Atom,
        INT,
        REAL,
        STRING,
        ID,
        LITERAL,
        INVALID
    }

    public static Dictionary<Symbol, string> patterns = new Dictionary<Symbol, string>()
    {
        {Symbol.LITERAL, @"^\s*[\(\)]"},
        {Symbol.REAL, @"^\s*[+-]?[0-9]*\.[0-9]+"},
        {Symbol.INT, @"^\s*[+-]?[0-9]+"},
        {Symbol.STRING, @"^\s*""(?>\\""|.)*"""},
        {Symbol.ID, @"^\s*[^\s""\(\)\.]+"}
    };

    public class Node
    {
        public Symbol symbol;
        public string lexeme;
        public List<Node> children;

        public Node(Symbol symbol, string lexeme = "", List<Node> children = null)
        {
            this.symbol = symbol;
            this.lexeme = lexeme;
            this.children = children ?? new List<Node>();
        }

        public void Print(string prefix = "")
        {
            Console.WriteLine($"{prefix}{this.symbol.ToString().PadRight(40-prefix.Length)} {this.lexeme}");
            foreach (var child in this.children)
            {
                child.Print(prefix + "  ");
            }
        }
    }

    static public List<Node> Tokenize(string src)
    {
        src = src.Replace("\n", "");
        List<Node> output = new List<Node>();
        int pos = 0;
        int size = src.Length;
        while (pos < size)
        {
            bool found = false;
            foreach(var kvp in patterns)
            {
                Regex expression = new Regex(kvp.Value);
                Match match = expression.Match(src.Substring(pos));
                if (match.Success)
                {
                    string element = match.Groups[0].Value;
                    output.Add(new Node(kvp.Key, element.Trim()));
                    pos = pos + element.Length;
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                throw new Exception("Lexer error.");
            }
        }
        return output;
    }

    public class Parser
    {
        public IEnumerator<Node> tokens;
        public Node token;
        public Node program;

        public Parser(Node[] token_list)
        {
            this.tokens = GetEnumerator(token_list);
            this.tokens.MoveNext();
            this.token = tokens.Current;
            this.program = this.Program();
        }

        public Node nextToken()
        {
            Node tok = this.token;
            if (this.tokens.MoveNext()) {
                this.token = this.tokens.Current;
            } else
            {
                this.token = null;
            }
            return tok;
        }

        public Node Program()
        {
            List<Node> children = new List<Node>();
            while (this.token != null)
            {
                children.Add(this.sexpr());
            }
            return new Node(Symbol.Program, "", children);
        }

        public Node sexpr()
        {
            List<Node> children = new List<Node>();
            if (this.token.lexeme == "(")
            {
                children.Add(this.list());
                return new Node(Symbol.SExpr, "", children);
            } else 
            {
                children.Add(this.atom());
                return new Node(Symbol.SExpr, "", children);
            }
        }

        public Node list()
        {
            List<Node> children = new List<Node>();
            Node lparen = this.nextToken();
            children.Add(lparen);
            if (this.token.lexeme == ")")
            {
                children.Add(this.nextToken());
                return new Node(Symbol.List, "", children);
            } else
            {
                Node seq = this.seq();
                if (this.token.lexeme == ")")
                {
                    children.Add(seq);
                    children.Add(this.nextToken());
                    return new Node(Symbol.List, "", children);
                } else
                {
                    throw new Exception("Syntax error.");
                }
            }
        }

        public Node seq()
        {
            List<Node> children = new List<Node>();
            Node sexpr = this.sexpr();
            children.Add(sexpr);
            if (this.token.lexeme == ")")
            {
                return new Node(Symbol.Seq, "", children);
            } else
            {
                Node subseq = this.seq();
                children.Add(subseq);
                return new Node(Symbol.Seq, "", children);
            }
        }

        public Node atom()
        {
            Symbol s = this.token.symbol;
            if ((s == Symbol.ID)||(s == Symbol.INT)||(s == Symbol.REAL)||(s == Symbol.STRING))
            {
                List<Node> children = new List<Node>();
                children.Add(this.nextToken());
                return new Node(Symbol.Atom, "", children);
            } else
            {
                throw new Exception("Syntax error.");
            }
        }

        public static IEnumerator<Node> GetEnumerator(Node[] node_list)
        {
            foreach (Node n in node_list)
            {
                yield return n;
            }
        }
    }

    static public Node Parse(Node[] tokens)
    {
        Parser p = new Parser(tokens);
	return p.program;
    }

    static private void CheckString(string lispcode)
    {
        try
        {
            Console.WriteLine(new String('=', 50));
            Console.Write("Input: ");
            Console.WriteLine(lispcode);
            Console.WriteLine(new String('-', 50));

            Node[] tokens = Tokenize(lispcode).ToArray();

            Console.WriteLine("Tokens");
            Console.WriteLine(new String('-', 50));
            foreach (Node node in tokens)
            {
                Console.WriteLine($"{node.symbol,-18}\t: {node.lexeme}");
            }
            Console.WriteLine(new String('-', 50));

            Node parseTree = Parse(tokens);

            Console.WriteLine("Parse Tree");
            Console.WriteLine(new String('-', 50));
            parseTree.Print();
            Console.WriteLine(new String('-', 50));
        }
        catch (Exception)
        {
            Console.WriteLine("Threw an exception on invalid input.");
        }
    }

    public static void Main(string[] args)
    {
        CheckString(Console.In.ReadToEnd());
    }
}

