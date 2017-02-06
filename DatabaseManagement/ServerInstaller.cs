using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;


namespace DatabaseManagement
{
	[RunInstaller(true)]
	public partial class ServerInstaller : System.Configuration.Install.Installer
	{
		public ServerInstaller()
		{
			InitializeComponent();
			this.AfterInstall += ServerInstaller_AfterInstall;
		}

		void ServerInstaller_AfterInstall(object sender, InstallEventArgs e)
		{
			CreateDatabase();
		}

		private void CreateDatabase()
		{
			
		}
	}
}
