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
	:	EOF																									#Eof
	| 'ls' files=F? dirs=D? compileUnit																						#Ls
	| 'cd' path=Path? ident=Identifier? compileUnit																			#Cd
	| ('copy' | 'cp') opt=R? src=Path dst=Path compileUnit													#Cp
	| ('createdisk' | 'cdisk') par1=optParams* '-s' (Integer SizeUnit | Size)  par2=optParams* compileUnit	#Cdisk
	| ('removedisk' | 'rmdisk') ('-p' sys=SysPath | '-n' name=String)+										#Rmdisk
	| 'mkdir' trgt=Path																						#Mkdir
	| ('remove' | 'rm') opt=R? trgt=Path																	#Rm
	| ('move' | 'mv') opt=R? src=Path dst=Path																#Mv
	| ('import' | 'im') ext=SysPath int=Path																#Im
	| ('export' | 'ex') int=Path ext=SysPath																#Ex
	| 'free'																								#Free
	| 'occ'																									#Occ
	;

optParams
	: '-p' Path																								#PathParam
	| '-n' String																							#StringParam
	;
/*
 * Lexer Rules
 */

 R : '-R';
 F : '-f';
 D : '-d';

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
	: ('/' String)+ ('/')? ('/' Identifier)?
	| '/'
	;

SysPath
	: [a-zA-Z]+ ':' ('\\' String)* ('\\')? ('\\' Identifier)?
	;

WS
	:	' ' -> channel(HIDDEN)
	;
