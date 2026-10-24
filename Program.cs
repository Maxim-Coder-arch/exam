/*
ответы на вопросы:

почему нужно использовать sql параметры?

для чего нужен DataContext в LINQ to sql?

что делает include() в ef core?



Использование SQL-параметров в ADO.NET критически важно по 
нескольким причинам. Прежде всего, параметры защищают от SQL-инъекций: 
когда вы конкатенируете строки для построения запроса, 
злоумышленник может внедрить произвольный SQL-код через пользовательский ввод, 
а параметры передаются отдельно от текста запроса и воспринимаются как данные, 
а не как исполняемый код. 
Кроме того, параметры позволяют СУБД кэшировать план выполнения запроса: 
при использовании разных значений с одинаковой структурой запроса с 
параметрами сервер может переиспользовать уже 
скомпилированный план, что повышает производительность, тогда как 
конкатенация создаёт каждый раз новый текст запроса и новый план. 
Параметры также корректно обрабатывают типы данных, форматы дат, 
специальные символы и значения NULL, избавляя от проблем с экранированием 
и региональными настройками, и упрощают чтение и поддержку кода.



DataContext в LINQ to SQL — это основной класс, который представляет собой 
точку входа для работы с базой данных и выступает как мост между объектной 
моделью и реляционной БД. Он содержит сведения о подключении (connection string), 
управляет соединением с базой, отслеживает изменения объектов (change tracking), 
транслирует LINQ-запросы в SQL и материализует результаты в объекты, а также 
реализует Unit of Work: вы вносите изменения в объекты, а DataContext накапливает 
их и при вызове SubmitChanges формирует и выполняет соответствующие 
INSERT/UPDATE/DELETE в рамках транзакции. Кроме того, через него настраиваются 
параметры логирования, отложенной загрузки и поведение при конфликтах параллелизма.



Метод Include() в EF Core предназначен для жадной загрузки (eager loading) 
связанных данных. По умолчанию навигационные свойства не загружаются вместе 
с основной сущностью, и обращение к ним приводит либо к ленивой загрузке 
(если она включена), либо к null. Include() указывает EF Core, что при 
выполнении запроса нужно добавить в генерируемый SQL JOIN (или отдельный запрос) 
и загрузить указанную навигацию в том же запросе, что избавляет от проблемы N+1 
запросов и лишних обращений к базе. Include можно применять к коллекциям и ссылочным 
свойствам, а также комбинировать с ThenInclude для загрузки вложенных уровней связей; 
при этом важно помнить, что Include влияет на форму и объём выбираемых данных, 
поэтому его следует использовать осознанно, не включая лишние связи без необходимости.
*/


using DatabaseCourseExam.AdoNet;
using System.Data;
using DatabaseCourseExam.EntityFramework;
using DatabaseCourseExam.DapperTasks;

DataTable? studentsTable = null;

while (true)
{
    Console.WriteLine();
    Console.WriteLine("=== Зачёт по работе с БД ===");
    Console.WriteLine("1. Показать студентов");
    Console.WriteLine("2. Изменить возраст студента");
    Console.WriteLine("3. Записать студента на курс");
    Console.WriteLine("4. Загрузить студентов в DataTable");
    Console.WriteLine("5. Изменить возраст в DataTable");
    Console.WriteLine("6. Показать DataTable");
    Console.WriteLine("7. Сохранить изменения DataTable");
    Console.WriteLine("8. EF Core: показать студентов");
    Console.WriteLine("9. EF Core: изменить возраст студента");
    Console.WriteLine("10. EF Core: показать курсы");
    Console.WriteLine("11. Dapper: показать студентов");
    Console.WriteLine("12. Dapper: изменить возраст");
    Console.WriteLine("13. Dapper: показать курсы");
    Console.WriteLine("14. Dapper: записать студента на курс");
    Console.WriteLine("0. Выход");
    Console.Write("Выберите действие: ");

    string? choice = Console.ReadLine();

    try
    {
        switch (choice)
        {
            case "1":
                await AdoNetTasks.ShowStudentsAsync();
                break;

            case "2":
                Console.Write("ID студента: ");
                int studentId = int.Parse(Console.ReadLine()!);

                Console.Write("Новый возраст: ");
                int age = int.Parse(Console.ReadLine()!);

                await AdoNetTasks.UpdateStudentAgeAsync(studentId, age);
                break;

            case "3":
                Console.Write("ID студента: ");
                int enrollmentStudentId = int.Parse(Console.ReadLine()!);

                Console.Write("ID курса: ");
                int courseId = int.Parse(Console.ReadLine()!);

                await AdoNetTasks.EnrollStudentAsync(
                    enrollmentStudentId,
                    courseId);
                break;

            case "4":
                studentsTable = await DisconnectedTasks.LoadStudentsAsync();
                Console.WriteLine("Данные загружены в память.");
                DisconnectedTasks.PrintTable(studentsTable);
                break;

            case "5":
                if (studentsTable is null)
                {
                    Console.WriteLine("Сначала загрузите данные (пункт 4).");
                    break;
                }

                Console.Write("ID студента: ");
                int disconnectedStudentId = int.Parse(Console.ReadLine()!);

                Console.Write("Новый возраст: ");
                int disconnectedAge = int.Parse(Console.ReadLine()!);

                if (disconnectedAge is < 14 or > 100)
                {
                    Console.WriteLine("Возраст должен быть от 14 до 100.");
                    break;
                }

                DisconnectedTasks.ChangeAgeInMemory(
                    studentsTable,
                    disconnectedStudentId,
                    disconnectedAge);
                break;

            case "6":
                if (studentsTable is null)
                {
                    Console.WriteLine("Сначала загрузите данные (пункт 4).");
                    break;
                }

                DisconnectedTasks.PrintTable(studentsTable);
                break;

            case "7":
                if (studentsTable is null)
                {
                    Console.WriteLine("Сначала загрузите данные (пункт 4).");
                    break;
                }

                DisconnectedTasks.SaveChanges(studentsTable);
                break;

            case "8":
                await EntityFrameworkTasks.ShowStudentsAsync();
                break;

            case "9":
                {
                    Console.Write("ID студента: ");

                    if (!int.TryParse(Console.ReadLine(), out int stId))
                    {
                        Console.WriteLine("Некорректный ID.");
                        break;
                    }

                    Console.Write("Новый возраст: ");

                    if (!int.TryParse(Console.ReadLine(), out int newAge)
                        || newAge is < 14 or > 100)
                    {
                        Console.WriteLine("Возраст должен быть от 14 до 100.");
                        break;
                    }

                    await EntityFrameworkTasks.UpdateStudentAgeAsync(
                        stId,
                        newAge);

                    break;
                }

            case "10":
                await EntityFrameworkTasks.ShowCoursesAsync();
                break;

            case "11":
                await DapperStudentTasks.ShowStudentsAsync();
                break;

            case "12":
                {
                    Console.Write("ID студента: ");

                    if (!int.TryParse(Console.ReadLine(), out int dStudentId))
                    {
                        Console.WriteLine("Некорректный ID.");
                        break;
                    }

                    Console.Write("Новый возраст: ");

                    if (!int.TryParse(Console.ReadLine(), out int newAge)
                        || newAge is < 14 or > 100)
                    {
                        Console.WriteLine("Возраст должен быть от 14 до 100.");
                        break;
                    }

                    await DapperStudentTasks.UpdateStudentAgeAsync(
                        dStudentId,
                        newAge);

                    break;
                }

            case "13":
                await DapperStudentTasks.ShowCoursesAsync();
                break;

            case "14":
                {
                    Console.Write("ID студента: ");

                    if (!int.TryParse(Console.ReadLine(), out int dStudentId))
                    {
                        Console.WriteLine("Некорректный ID.");
                        break;
                    }

                    Console.Write("ID курса: ");

                    if (!int.TryParse(Console.ReadLine(), out int dCourseId))
                    {
                        Console.WriteLine("Некорректный ID.");
                        break;
                    }

                    await DapperStudentTasks.EnrollStudentAsync(
                        dStudentId,
                        dCourseId);

                    break;
                }
            case "0":
                return;

            default:
                Console.WriteLine("Неизвестная команда.");
                break;
        }
    }
    catch (FormatException)
    {
        Console.WriteLine("Введите целое число.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Ошибка: {ex.Message}");
    }
}