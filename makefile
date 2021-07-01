psay_hello:
	echo "Hello World"

build:
	dotnet build src/SmileyBot.sln

test:
	dotnet test src/SmileyBot.sln

install:
	dotnet tool update -g dotnet-format --version 5.1.225507
	find .git/hooks -type l -exec rm {} \;
	find .githooks -type f -exec ln -sf ../../{} .git/hooks/ \;

run:
	dotnet run --project src/SmileyBot.Console/SmileyBot.Console.csproj
