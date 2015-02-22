namespace MonoServer
{
	public interface IDatabaseProvider<T>
	{
		/// <summary>
		/// Initialize this instance. Throws Exception when Database could not be initialized
		/// </summary>
		void Initialize ();

		/// <summary>
		/// Gets the database.
		/// </summary>
		/// <returns>The database.</returns>
		T GetDatabase();

		/// <summary>
		/// Close this instance and enventual database connections.
		/// </summary>
		void Close();
	}
}

