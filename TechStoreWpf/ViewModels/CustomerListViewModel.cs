﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Data.Entity;
using System.Threading.Tasks;
using System.Windows.Input;
using TechStoreLibrary.Database;
using TechStoreLibrary.Enums;
using TechStoreLibrary.Models;
using TechStoreWpf.Helpers;
using TechStoreWpf.ViewModels.Base;
using TechStoreWpf.Views;
using System.Windows;
using System.Threading;
using TechStoreWpf.Views.Windows;

namespace TechStoreWpf.ViewModels
{
    public class CustomerListViewModel : BaseViewModel
    {
        #region Attributes
        private CustomerListView customerListView;
        private Window waitWindow;
        private ObservableCollection<Customer> customers;
        #endregion

        #region Properties
        /// <summary>
        /// Customer list view.
        /// </summary>
        public CustomerListView CustomerListView
        {
            get
            {
                return customerListView;
            }
            set
            {
                customerListView = value;
            }
        }

        /// <summary>
        /// Data loading window.
        /// </summary>
        public Window WaitWindow
        {
            get
            {
                return waitWindow;
            }
            set
            {
                waitWindow = value;
            }
        }

        /// <summary>
        /// List of customers.
        /// </summary>
        public ObservableCollection<Customer> Customers
        {
            get
            {
                return customers;
            }
            set
            {
                customers = value;
                OnPropertyChanged();
            }
        }

        public ICommand AddCustomerCommand { get; private set; }
        public ICommand EditCustomerCommand { get; private set; }
        public ICommand DeleteCustomerCommand { get; private set; }
        #endregion

        #region Constructors
        public CustomerListViewModel(CustomerListView customerListView)
        {
            WaitWindow = new WaitWindow();
            CustomerListView = customerListView;

            Task.Run(() => LoadCustomersAsync());
            
            AddCustomerCommand = new RelayCommand(ExecAddCustomer, CanAddCustomer);
            EditCustomerCommand = new RelayCommand(ExecEditCustomer, CanEditCustomer);
            DeleteCustomerCommand = new RelayCommand(ExecDeleteCustomerAsync, CanDeleteCustomer);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Loads all customers.
        /// </summary>
        public async void LoadCustomersAsync()
        {
            #pragma warning disable CS4014
            Application.Current.Dispatcher.BeginInvoke(new ThreadStart(() =>
            {
                try
                {
                    WaitWindow.ShowDialog();
                }
                catch (InvalidOperationException e)
                {

                }

            }));
            #pragma warning restore CS4014

            App.SetConnectionResource();

            switch (App.DataSource)
            {
                case ConnectionResource.LOCALAPI:
                    Customers = new ObservableCollection<Customer>(await new WebServiceManager<Customer>().GetAllAsync());
                    break;
                case ConnectionResource.LOCALMYSQL:
                    using (var ctx = new MysqlDbContext(ConnectionResource.LOCALMYSQL))
                    {
                        Customers = new ObservableCollection<Customer>(await ctx.DbSetCustomers.Include(c => c.Address).ToListAsync());
                    }
                    break;
                default:
                    break;
            }

            await Application.Current.Dispatcher.BeginInvoke(new ThreadStart(() =>
            {
                WaitWindow.Close();
            }));
        }

        private bool CanAddCustomer(object obj)
        {
            return true;
        }

        /// <summary>
        /// Navigates to the customer form view.
        /// </summary>
        /// <param name="obj"></param>
        private void ExecAddCustomer(object obj)
        {
            CustomerListView.NavigationService.Navigate(new CustomerView());
        }

        /// <summary>
        /// Edition is active only when a customer is selected.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private bool CanEditCustomer(object obj)
        {
            if (CustomerListView.CustomerListUserControl.CustomerList.SelectedIndex != -1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Navigates to the customer form view in edit mode.
        /// </summary>
        /// <param name="obj"></param>
        private void ExecEditCustomer(object obj)
        {
            CustomerView customerView = new CustomerView((Customer)CustomerListView.CustomerListUserControl.CustomerList.SelectedItem);
            CustomerListView.NavigationService.Navigate(customerView);
        }

        /// <summary>
        /// Deletion is active only when a customer is selected.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private bool CanDeleteCustomer(object obj)
        {
            if (CustomerListView.CustomerListUserControl.CustomerList.SelectedIndex != -1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Deletes selected customer.
        /// </summary>
        /// <param name="obj"></param>
        private async void ExecDeleteCustomerAsync(object obj)
        {
            Customer customer = (Customer)CustomerListView.CustomerListUserControl.CustomerList.SelectedItem;
            Customers.Remove(customer);

            switch (App.DataSource)
            {
                case ConnectionResource.LOCALAPI:
                    await new WebServiceManager<Customer>().DeleteAsync(customer);
                    break;
                case ConnectionResource.LOCALMYSQL:
                    using (var ctx = new MysqlDbContext(ConnectionResource.LOCALMYSQL))
                    {
                        ctx.DbSetCustomers.Attach(customer);
                        ctx.DbSetCustomers.Remove(customer);
                        await ctx.SaveChangesAsync();
                    }
                    break;
                default:
                    break;
            }
        }
        #endregion
    }
}
