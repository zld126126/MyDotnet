echo "Gen Start!!!"
cd ./netcoreapp3.1
echo "cd netcoreapp3.1"
rm -rf ./Gen
mkdir Gen
echo "mkdir Gen"
dotnet MyXluaGenerate.dll MyDotXlua.dll
echo "dotnet Gen"
cp -R ./Gen ../MyDotXlua
echo "Gen End!!!"
echo "Please Close This Window..."
read -n 1
