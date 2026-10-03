using System;

namespace MegaCrit.Sts2.Core.Models.Exceptions;

public class DuplicateModelException : Exception
{
	public DuplicateModelException(string message)
		: base(message)
	{
	}
}
