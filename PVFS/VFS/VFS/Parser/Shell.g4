grammar Shell;

@parser::members
{
	protected const int EOF = Eof;
}

@lexer::members
{
	protected const int EOF = Eof;
	protected const int HIDDEN = Hidden;
}

/*
 * Parser Rules
 */

compileUnit
	:	EOF
	| 'ls' compileUnit
	| 'cd' Path compileUnit
	| ('copy' | 'cp') ('-R')? Path Path compileUnit
	| ('createdisk' | 'cdisk') optParams* '-s' (Integer SizeUnit | Size)  optParams* compileUnit
	| ('removedisk' | 'rmdisk') ('-p' SysPath | '-n' String)+
	| 'mkdir' (Path | Identifier)
	| ('remove' | 'rm') ('-R')? (Path | Identifier)+
	| ('move' | 'mv') ('-R')? (Path | Identifier) (Path | Identifier)
	| ('import' | 'im') SysPath (Path | Identifier)
	| ('export' | 'ex') (Path | Identifier) SysPath
	| 'free'
	| 'occ'
	;

optParams
	: '-p' Path
	| '-n' String
	;
/*
 * Lexer Rules
 */
 SizeUnit
	:'kb' | 'mb' | 'gb' | 'tb' | 'KB' | 'MB' | 'GB' | 'TB';

Size
	: Integer SizeUnit
	;
 Integer
	: [0-9]+
	;

 Identifier
	: String '.' String
	;

String
	: [a-zA-Z0-9]+
	;

Path
	: ('/' String)+ ('/')?
	| '/'
	;

SysPath
	: [a-zA-Z]+ ':' ('\\' String)* ('\\')?
	;

WS
	:	' ' -> channel(HIDDEN)
	;
