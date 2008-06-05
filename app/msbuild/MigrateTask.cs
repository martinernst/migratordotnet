#region License
//The contents of this file are subject to the Mozilla Public License
//Version 1.1 (the "License"); you may not use this file except in
//compliance with the License. You may obtain a copy of the License at
//http://www.mozilla.org/MPL/
//Software distributed under the License is distributed on an "AS IS"
//basis, WITHOUT WARRANTY OF ANY KIND, either express or implied. See the
//License for the specific language governing rights and limitations
//under the License.
#endregion

using System;
using System.Reflection;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using Migrator.Compile;
using Migrator.MSBuild.Logger;

namespace Migrator.MSBuild
{
	/// <summary>
	/// Runs migrations on a database
	/// </summary>
	/// <example>
    /// <Target name="Migrate" DependsOnTargets="Build">
    ///     <Migrate Provider="SqlServer"
    ///         Connectionstring="Database=MyDB;Data Source=localhost;User Id=;Password=;"
    ///         Migrations="bin/MyProject.dll"/>
    /// </Target>
	/// </example>
    /// <example>
    /// <Target name="Migrate" DependsOnTargets="Build">
    ///     <CreateProperty Value="-1"  Condition="'$(SchemaVersion)'==''">
    ///        <Output TaskParameter="Value" PropertyName="SchemaVersion"/>
    ///     </CreateProperty>
    ///     <Migrate Provider="SqlServer"
    ///         Connectionstring="Database=MyDB;Data Source=localhost;User Id=;Password=;"
    ///         Migrations="bin/MyProject.dll"
    ///         To="$(SchemaVersion)"/>
    /// </Target>
    /// </example>
	public class Migrate : Task
	{
		private int _to = -1; // To last revision
		private string _provider;
		private string _connectionString;
		private ITaskItem[] _migrationsAssembly;
		private bool _trace;

        private string _directory;
        private string _language;

		[Required]
		public string Provider
		{
			set { _provider = value; }
			get { return _provider; }
		}

        [Required]
		public string ConnectionString
		{
			set { _connectionString = value; }
			get { return _connectionString; }
		}

        /// <summary>
        /// The paths to the assemblies that contain your migrations. 
        /// This will generally just be a single item.
        /// </summary>
        public ITaskItem[] Migrations
		{
			set { _migrationsAssembly = value; }
			get { return _migrationsAssembly; }
		}

        /// <summary>
        /// The paths to the directory that contains your migrations. 
        /// This will generally just be a single item.
        /// </summary>
        public string Directory
        {
            set { _directory = value; }
            get { return _directory; }
        }

        public string Language
        {
            set { _language = value; }
            get { return _language; }
        }

		public int To
		{
			set { _to = value; }
			get { return _to; }
		}

		public bool Trace
		{
			set { _trace = value; }
			get { return _trace; }
		}

		public override bool Execute()
		{
            if (! String.IsNullOrEmpty(Directory))
            {
                ScriptEngine engine = new ScriptEngine(Language, null);
                Execute(engine.Compile(Directory));
            }

            if (null != Migrations)
            {
                foreach (ITaskItem assembly in Migrations)
                {
                    Assembly asm = Assembly.LoadFrom(assembly.GetMetadata("FullPath"));
                    Execute(asm);
                }
            }

		    return true;
		}

        private void Execute(Assembly asm)
	    {
	        Migrator mig = new Migrator(Provider, ConnectionString, asm, Trace, new TaskLogger(this));

	        if (_to == -1)
	            mig.MigrateToLastVersion();
	        else
	            mig.MigrateTo(_to);
	    }
	}
}


