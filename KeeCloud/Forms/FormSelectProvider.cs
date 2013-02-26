﻿using KeeCloud.Providers;
using System;
using System.Linq;
using System.Windows.Forms;

namespace KeeCloud.Forms
{
    public partial class FormSelectProvider : UserControl
    {
        private class DataItem
        {
            public DataItem(ProviderItem wr)
            {
                this.Provider = wr.Create(new Uri(wr.Protocol + @"://"));
                this.FriendlyName = this.Provider.FriendlyName;
            }

            public string FriendlyName { get; set; }
            public IProvider Provider { get; set; }
        }

        public IProvider SelectedProvider { get; private set; }

        public FormSelectProvider()
        {
            InitializeComponent();
            this.comboBoxProviders.DisplayMember = "FriendlyName";
            this.comboBoxProviders.DataSource = ProviderRegistry.SupportedWebRequests.Select(wr => new DataItem(wr)).Where(wr => wr.Provider.CanConfigureCredentials).ToArray();
        }

        private void comboBoxProviders_SelectedIndexChanged(object sender, EventArgs e)
        {
            var item = (DataItem)this.comboBoxProviders.SelectedItem;
            this.SelectedProvider = item.Provider;
        }
    }
}