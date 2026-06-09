rm before.txt after.txt &>/dev/null

dotnet run before.txt after.txt &>/dev/null

diff -y before.txt after.txt > tree_diffs.txt

rm before.txt after.txt
