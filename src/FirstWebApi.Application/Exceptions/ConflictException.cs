namespace FirstWebApi.Application.Exceptions;

public class ConflictException(string message) : Exception(message);
