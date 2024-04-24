using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using sotrudniki.Helper;
using sotrudniki.Model;
using sotrudniki.View;

namespace sotrudniki.ViewModel
{
    public class PersonViewModel : INotifyPropertyChanged
    {
        readonly string path = @"C:\Users\у\Desktop\bhwc-v2\sotrudniki\DataModels\PersonData.json";
        private PersonDpo _selectedPersonDpo;
        /// <summary>
        /// выделенные в списке данные по сотруднику
        /// </summary>
        public PersonDpo SelectedPersonDpo
        {
            get { return _selectedPersonDpo; }
            set
            {
                _selectedPersonDpo = value;
                OnPropertyChanged("SelectedPersonDpo");
            }
        }
        /// <summary>
        /// коллекция данных по сотрудникам
        /// </summary>
        public ObservableCollection<Person> ListPerson { get; set; }
        public ObservableCollection<PersonDpo> ListPersonDpo
        {
            get;
            set;
        }
        string _jsonPersons = String.Empty;
        public string Error { get; set; }
        public string Message { get; set; }
        public PersonViewModel()
        {
            ListPerson = new ObservableCollection<Person>();
            ListPersonDpo = new ObservableCollection<PersonDpo>();
            ListPerson = LoadPerson();
            ListPersonDpo = GetListPersonDpo();
        }
 #region AddPerson

 private RelayCommand _addPerson;
       
        public RelayCommand AddPerson
        {
            get
            {
                return _addPerson ??
                (_addPerson = new RelayCommand(obj =>
                {
                    WindowNewEmployee wnPerson = new WindowNewEmployee
                    {
                        Title = "Новый сотрудник"
                    };
                    // формирование кода нового собрудника
                    int maxIdPerson = MaxId() + 1;
                    PersonDpo per = new PersonDpo
                    {
                        Id = maxIdPerson,
                        Birthday = DateTime.Now.ToString(),
                    };

                    wnPerson.DataContext = per;
                    if (wnPerson.ShowDialog() == true)
                    {
                        var r = (Role)wnPerson.CbRole.SelectedValue;
                        if (r != null)
                        {
                            per.RoleName = r.NameRole;
                            per.Birthday = per.Birthday;
                            ListPersonDpo.Add(per);
                            // добавление нового сотрудника в коллекциюListRoles<Person>
                            Person p = new Person();
                            p = p.CopyFromPersonDPO(per);
                            ListPerson.Add(p);
                            try
                            {
                                // сохранение изменений в файле json
                                SaveChanges(ListPerson);
                            }
                            catch (Exception e)
                            {
                                Error = "Ошибка добавления данных в json файл\n" +
                e.Message;
                            }
                        }
                    }
                    


                },
                (obj) => true));
            }
        }
        #endregion
        #region EditPerson
        /// команда редактирования данных по сотруднику
        private RelayCommand _editPerson;
        public RelayCommand EditPerson
        {
            get
            {
                return _editPerson ??
                (_editPerson = new RelayCommand(obj =>
                {
                    WindowNewEmployee wnPerson = new WindowNewEmployee()
                    {
                        Title = "Редактирование данных сотрудника",
                    };
                    PersonDpo personDpo = SelectedPersonDpo;
                    var tempPerson = personDpo.ShallowCopy();
                    wnPerson.DataContext = tempPerson;

                    if (wnPerson.ShowDialog() == true)
                    {
                        // сохранение данных в оперативной памяти
                        // перенос данных из временного класса в класс отображения данных
                        var r = (Role)wnPerson.CbRole.SelectedValue;
                        if (r != null)
                        {
                            personDpo.RoleName = r.NameRole;
                            personDpo.FirstName = tempPerson.FirstName;
                            personDpo.LastName = tempPerson.LastName;
                            personDpo.Birthday = tempPerson.Birthday;
                            // перенос данных из класса отображения данных в класс Person


                var per = ListPerson.FirstOrDefault(p => p.Id == personDpo.Id);
                            if (per != null)
                            {
                                per = per.CopyFromPersonDPO(personDpo);
                            }
                            try
                            {
                                // сохраненее данных в файле json
                                SaveChanges(ListPerson);
                            }
                            catch (Exception e)
                            {


                Error = "Ошибка редактирования данных в json файл\n"
               + e.Message;
                            }
                        }
                        else
                        {
                            Message = "Необходимо выбрать должность сотрудника.";
                        }
                    }
                }, (obj) => SelectedPersonDpo != null && ListPersonDpo.Count > 0));
            }
        }
        #endregion
        #region DeletePerson
        /// команда удаления данных по сотруднику
        private RelayCommand _deletePerson;
        public RelayCommand DeletePerson
        {
            get
            {
                return _deletePerson ??
                (_deletePerson = new RelayCommand(obj =>
                {
                    PersonDpo person = SelectedPersonDpo;
                    MessageBoxResult result = MessageBox.Show("Удалить данные по сотруднику: \n" +


     person.LastName + " " + person.FirstName,
     "Предупреждение", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                    if (result == MessageBoxResult.OK)
                    {
                        try
                        {
                            // удаление данных в списке отображения данных
                            ListPersonDpo.Remove(person);
                            // поиск удаляемого класса в коллекции ListRoles
                            var per = ListPerson.FirstOrDefault(p => p.Id == person.Id);
                            if (per != null)
                            {
                                ListPerson.Remove(per);
                                // сохраненее данных в файле json
                                SaveChanges(ListPerson);
                            }
                        }
                        catch (Exception e)
                        {
                            Error = "Ошибка удаления данных\n" + e.Message;
                        }
                    }
                  


                }, (obj) => SelectedPersonDpo != null && ListPersonDpo.Count > 0));
            }
        }
        #endregion
        #region Method
        /// <summary>
        /// Загрузка данных по сотрудникам из json файла
        /// </summary>
        /// <returns></returns>
        public ObservableCollection<Person> LoadPerson()
        {
            _jsonPersons = File.ReadAllText(path);
            if (_jsonPersons != null)
            {
                ListPerson = JsonConvert.DeserializeObject<ObservableCollection<Person>>(_jsonPersons);
                return ListPerson;
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// Формирование коллекции классов PersonDpo из коллекции Person
        /// </summary>
        /// <returns></returns>
        public ObservableCollection<PersonDpo> GetListPersonDpo()
        {
            foreach (var person in ListPerson)
            {
                PersonDpo p = new PersonDpo();
                p = p.CopyFromPerson(person);
                ListPersonDpo.Add(p);
            }
            return ListPersonDpo;
        }
        /// <summary>
        /// Нахождение максимального Id в коллекции данных
        /// </summary>
        /// <returns></returns>
        public int MaxId()
        {
            int max = 0;
            foreach (var r in this.ListPerson)
            {
                if (max < r.Id)
                {
                    max = r.Id;
                };
           
            }
            return max;
        }
        /// <summary>
        /// Сохранение json-строки с данными по сотрудникам в json      
        /// </summary>
        /// <param name="listPersons"></param>
        private void SaveChanges(ObservableCollection<Person> listPersons)
        {
            var jsonPerson = JsonConvert.SerializeObject(listPersons);
            try
            {
                using (StreamWriter writer = File.CreateText(path))
                {
                    writer.Write(jsonPerson);
                }
            }
            catch (IOException e)
            {
                Error = "Ошибка записи json файла /n" + e.Message;
            }
        }
        #endregion
        public event PropertyChangedEventHandler PropertyChanged;
      
        protected virtual void OnPropertyChanged([CallerMemberName]
string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
