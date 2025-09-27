using System.Text;

/// <summary>
/// 한국어 조사를 자동으로 처리해주는 정적 헬퍼 클래스입니다.
/// </summary>
public static class KoreanPostpositionHelper
{
    /// <summary>
    /// 주어진 단어에 '을' 또는 '를'을 붙여 반환합니다.
    /// </summary>
    /// <param name="word">조사를 붙일 단어</param>
    /// <returns>조사가 붙은 완전한 문자열</returns>
    public static string Add_Eul_Reul(string word)
    {
        string postposition = HasFinalConsonant(word) ? "을" : "를";
        return $"{word}{postposition}";
    }

    /// <summary>
    /// 주어진 단어에 '은' 또는 '는'을 붙여 반환합니다.
    /// </summary>
    /// <param name="word">조사를 붙일 단어</param>
    /// <returns>조사가 붙은 완전한 문자열</returns>
    public static string Add_Eun_Neun(string word)
    {
        string postposition = HasFinalConsonant(word) ? "은" : "는";
        return $"{word}{postposition}";
    }

    /// <summary>
    /// 문자열의 마지막 글자에 받침(종성)이 있는지 확인합니다.
    /// </summary>
    /// <param name="str">검사할 문자열</param>
    /// <returns>받침이 있으면 true, 없으면 false</returns>
    public static bool HasFinalConsonant(string str)
    {
        if (string.IsNullOrEmpty(str)) return false;

        char lastChar = str[str.Length - 1];

        // 한글 음절 범위(가-힣)인지 확인
        if (lastChar >= '가' && lastChar <= '힣')
        {
            // 유니코드에서 한글 음절의 시작(0xAC00)을 빼고 28로 나눈 나머지가 0이 아니면 받침이 있음
            return (lastChar - 0xAC00) % 28 != 0;
        }
        return false; // 한글이 아니면 받침 없는 것으로 처리
    }
}
