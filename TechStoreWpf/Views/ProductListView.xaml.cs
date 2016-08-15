﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TechStoreWpf.ViewModels;

namespace TechStoreWpf.Views
{
    /// <summary>
    /// Interaction logic for ProductListView.xaml
    /// </summary>
    public partial class ProductListView : Page
    {
        #region Attributes
        private ProductListViewModel productListViewModel;
        #endregion

        #region Properties
        /// <summary>
        /// ViewModel of the view.
        /// </summary>
        public ProductListViewModel ProductListViewModel
        {
            get
            {
                return productListViewModel;
            }
            set
            {
                productListViewModel = value;
            }
        }
        #endregion

        #region Constructors
        public ProductListView()
        {
            InitializeComponent();

            ProductListViewModel = new ProductListViewModel(this);
            DataContext = ProductListViewModel;
        }

        public ProductListView(string activeTab)
            : this()
        {
            TabControl productsTabControl = ProductListViewModel.ProductListView.ProductListUserControl.Products;
            foreach (TabItem tab in productsTabControl.Items)
            {
                if ((string)tab.Header == activeTab)
                    tab.IsSelected = true;
            }
        }
        #endregion

        #region Methods

        #endregion
    }
}
