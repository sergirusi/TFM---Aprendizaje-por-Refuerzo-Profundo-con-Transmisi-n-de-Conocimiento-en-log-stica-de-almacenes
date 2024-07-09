public interface IAgentObserver
{
    void OnEndEpisode(float reward);

    void OnLessonChanged(float lessonValue);
}
