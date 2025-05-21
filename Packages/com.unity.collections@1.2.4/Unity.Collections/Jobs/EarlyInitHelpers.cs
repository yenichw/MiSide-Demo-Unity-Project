using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Jobs
{
	public class EarlyInitHelpers
	{
		public delegate void EarlyInitFunction();

		private static List<EarlyInitFunction> s_PendingDelegates;

		public static void FlushEarlyInits()
		{
			while (s_PendingDelegates != null)
			{
				List<EarlyInitFunction> list = s_PendingDelegates;
				s_PendingDelegates = null;
				for (int i = 0; i < list.Count; i++)
				{
					try
					{
						list[i]();
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
					}
				}
			}
		}

		public static void AddEarlyInitFunction(EarlyInitFunction f)
		{
			if (s_PendingDelegates == null)
			{
				s_PendingDelegates = new List<EarlyInitFunction>();
			}
			s_PendingDelegates.Add(f);
		}

		public static void JobReflectionDataCreationFailed(Exception ex, Type jobType)
		{
			Debug.LogError($"Failed to create job reflection data for type ${jobType}:");
			Debug.LogException(ex);
		}
	}
}
