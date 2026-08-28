  > This save could not be loaded because it is damaged or from an unsupported version.

  A useful separation is:

  // Persistence/domain layer:
  throw new InvalidDataException(...);

  // Application/UI layer:
  public void LoadGame(string slot)
  {
      try
      {
          EnterGame(UserData.LoadGame(slot));
      }
      catch (InvalidDataException exception)
      {
          ShowLoadError(exception.Message);
      }
      catch (IOException exception)
      {
          ShowLoadError(exception.Message);
      }
  }

  The application layer should log the detailed exception and show a concise message or dialog. It should not silently start a new game or overwrite the bad save, because that could destroy
  recoverable data.
 