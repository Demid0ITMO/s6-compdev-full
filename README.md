Работоспособность можно проверить с помощью

```bash
cd Lab8
rm after.txt before.txt dump.txt &>/dev/null
dotnet run
```

Проверить оптимизатор (Будут выведены AST до и после оптимизации)
```bash
diff -y before.txt after.txt
```