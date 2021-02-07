
    public interface GameManagerObserver
    {
        void NotifyRoundBeginning(GameManager manager);
        void NotifyGameEnding(GameManager manager, GameEndingReason reason);
    }
    public enum GameEndingReason
    {
        LOSS, GOOD_WIN, BAD_WIN
    }