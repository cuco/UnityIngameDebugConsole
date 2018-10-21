using System;

namespace IngameDebugConsole
{
	[AttributeUsage( AttributeTargets.Method, Inherited = false, AllowMultiple = true )]
	public class ConsoleMethodAttribute : Attribute
	{
		private string m_command;
		private string m_description;

        public string RenameCommand { get { return m_command; } set { m_command = value; }}
        public string Description { get { return m_description; } set { m_description = value; }}

        public ConsoleMethodAttribute()
        {
            
        }
	} }