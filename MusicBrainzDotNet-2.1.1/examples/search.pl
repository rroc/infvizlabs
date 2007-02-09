#!/usr/bin/perl

opendir DIR, ".";     # . is the current directory

print("Files in the Directory: \n");

$datafile="excel.txt";
open(DAT,">$datafile") || die("Cannot Open File");


while ( $filename = readdir(DIR) ) 
	{
	$linedata = parseFile( $filename );	
	print DAT "$linedata";
	}
close(DAT);
closedir DIR;

#Find the info for FreeDB text files
# DTITLE=Charles Mingus / Town Hall Concert
# DYEAR=1964
# DGENRE=Jazz
sub parseFile
{
	$fn = $_[0];
#	print($fn.":\n");
	open(file, $fn);
	@raw_data=<file>;
	close file;

	$result = "";
	#browse file 
		foreach $line (@raw_data)
			{
			if( $line =~ /^DTITLE=(.+) \/ (.*)\n/ )
				{
				$result .= "$1\t$2";
				}
			elsif( $line =~ /^DGENRE=(.*)\n/ )
				{
				$result .= "\t$1";
				}
#			elsif( $line =~ /^DYEAR=(.*)\n/ )
#				{
#				$result .= "\t$1";
#				}				
			}
	return $result."\r\n";
}

