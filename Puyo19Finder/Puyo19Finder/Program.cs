using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Puyo19Finder
{
    class Program
    {
        //목표 연쇄
        //조건 때문에 19가 아니면 오히려 오래 걸림
        const int DEST_RENSA = 19;
        //n번째 모형마다 출력
        const int COUNT_CYCLE = 1;

        //찾아낸 모형 수
        static int m_FindCount;

        //뿌요 필드
        static byte[,] m_PuyoField;
        //연쇄 시뮬레이터
        static Simulator m_Simulator;

        static void Main(string[] args)
        {
            //변수 초기화
            m_FindCount = 0;

            m_PuyoField = new byte[6, 13];
            m_Simulator = new Simulator();
            //m_FieldChecked = new bool[6, 12]; //안씀

            m_PuyoCount = new int[6];
            m_PuyoShape = new int[]{0b01011,
                0b1000001010, 0b0110001010, 0b0100001010,
                0b0110101001, 0b0100101001, 0b0010101001,
                0b011000110001001, 0b010000100001001,
                0b0101001000, 0b0011001000, 0b0001001000,
                0b011000100101000, 0b010000100101000, 0b010000010101000, 0b001000010101000,
                0b010010100001000, 0b001010100001000,
                0b01000010000100001000}; //추후 설명

            m_Rensa = 0;
            m_Complete = false;
            //탐색 시작
            FindRensa(0);
        }


        //각 열마다 올려진 뿌요의 수
        static int[] m_PuyoCount;
        //모든 가능한 4개 터지는 모양
        //테트리스 미노들의 회전을 포함한 모양을 생각하면 쉽다. 총 19개
        static int[] m_PuyoShape;

        //현재 연쇄 수
        static int m_Rensa;
        //유저가 q를 입력해서 프로그램을 종료해야 하는가
        static bool m_Complete;

        //location : 다음에 추가할 4개 뿌요의 색과 위치 정보가 포함된 정수
        //0 ~ 3bit : 첫 번째 뿌요의 행, 4 ~ 6bit : 첫 번째 뿌요의 열
        //7 ~ 10bit : 두 번째 뿌요의 행, 11 ~ 13bit : 두 번째 뿌요의 열
        //14 ~ 17bit : 세 번째 뿌요의 행, 18 ~ 20bit : 세 번째 뿌요의 열
        //21 ~ 24bit : 네 번째 뿌요의 행, 25 ~ 27bit : 네 번째 뿌요의 열
        //28 ~ 29bit : 뿌요의 색
        static void FindRensa(int location)
        {
            //처음이 아니라면...
            if (location != 0)
            {
                //location에서 색을 뽑아낸다.
                byte color = (byte)((location >> 28) + 1);
                for (int i = 0; i < 4; i++)
                {
                    //location에서 각 뿌요의 행과 열을 뽑아낸다.
                    int loc = (location >> (i * 7)) & 127;
                    int x = loc >> 4;
                    int y = loc & 15;

                    //뿌요 필드의 해당 위치에 뿌요를 추가하고, 원래 있던 뿌요를 위로 올린다.
                    //각 열마다 올려진 뿌요의 수를 1 더한다.
                    for (int j = m_PuyoCount[x]; j > y; j--)
                        m_PuyoField[x, j] = m_PuyoField[x, j - 1];
                    m_PuyoField[x, y] = color;
                    m_PuyoCount[x]++;
                }

                //이번 뿌요를 추가해서 목표 연쇄 수에 도달하지 않는다면...
                if (m_Rensa < DEST_RENSA - 1)
                {
                    //뿌요를 사용할 수 없는 공간이 생겼거나 연쇄가 폭발한다면...
                    if (!IsChainable() || !m_Simulator.CheckRensa(m_PuyoField, m_Rensa + 1, 0))
                    {
                        
                        //뿌요 필드를 원래대로 되돌린다.
                        RevertField(location);
                        return;
                    }

                    //성공적으로 연쇄가 추가되었다. 연쇄 수를 1 올린다.
                    m_Rensa++;
                }
                else //도달한다면...
                {
                    //연쇄가 폭발하지 않고 조건에 부합하는 위치에서 발화가 시작된다면...
                    if (m_Simulator.CheckRensa(m_PuyoField, m_Rensa + 1, location))
                    {
                        //조건에 부합하는 모형을 발견했다. 
                        m_FindCount++;
                        //화면에 표시해야 되는 차례라면...
                        if (m_FindCount % COUNT_CYCLE == 0)
                        {
                            //연쇄 모형을 출력한다.
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.WriteLine("조건에 부합하는 모형을 찾았습니다!");
                            Console.WriteLine(m_FindCount + "번째 " + (m_Rensa + 1) + "연쇄 모형입니다.");

                            for (int j = 12; j >= 0; j--)
                            {
                                for (int i = 0; i < 6; i++)
                                {
                                    switch (m_PuyoField[i, j])
                                    {
                                        case 1:
                                            Console.ForegroundColor = ConsoleColor.Red;
                                            Console.Write("○");
                                            break;
                                        case 2:
                                            Console.ForegroundColor = ConsoleColor.Green;
                                            Console.Write("○");
                                            break;
                                        case 3:
                                            Console.ForegroundColor = ConsoleColor.Blue;
                                            Console.Write("○");
                                            break;
                                        case 4:
                                            Console.ForegroundColor = ConsoleColor.Yellow;
                                            Console.Write("○");
                                            break;
                                        default:
                                            Console.ForegroundColor = ConsoleColor.White;
                                            Console.Write("__");
                                            break;
                                    }
                                }
                                Console.WriteLine();
                            }

                            //q를 입력받으면 m_Complete를 true로 바꿔 프로그램을 종료한다.
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.WriteLine("그만 찾기를 원하시면 q를 입력해 주세요.");
                            if (Console.ReadLine().Equals("q"))
                            {
                                m_Complete = true;
                                return;
                            }
                        }
                    }

                    //뭐가 어쨌든, 목표 연쇄 이상의 연쇄는 찾지 않는다. 필드를 원래대로 되돌린다.
                    RevertField(location);
                    return;
                }
            }

            //연쇄 가능성이 있도록 뿌요 4개를 추가해서, 다시 이 메소드를 호출한다.
            //특정 위치를 '기준 위치'로 정한 다음, 해당 위치의 뿌요가 터지는 가능한 모든 모양을 테스트해 본다.
            //기준 위치는 항상 모양의 맨 왼쪽 아래에 있다.

            //모든 가능한 모양의 수
            int shapeCount = m_PuyoShape.Length;
            //1열 ~ 6열의 모든 위치를 테스트한다.
            for (int i = 0; i < 6; i++)
            {
                //해당 열에 올려져 있는 뿌요의 수
                int puyoCount = m_PuyoCount[i];
                //3열은 최대 11개까지만 올라갈 수 있고, 다른 열은 최대 13개까지 올라갈 수 있다.
                int maxCount = i == 2
                    ? 11
                    : 13;

                //터지는 뿌요가 공중에 떠 있을 수는 없으므로, 이미 올려진 뿌요의 바로 위까지만 생각한다.
                for (int j = 0; j <= puyoCount; j++)
                {
                    //모든 모양을 테스트한다.
                    for (int k = 0; k < shapeCount; k++)
                    {
                        //m_PuyoShape에 저장된 정수의 의미는 다음과 같다.
                        //0 ~ 4bit : 기준 열의 정보
                        //5 ~ 9bit : 기준+1 열의 정보
                        //10 ~ 14bit : 기준+2 열의 정보
                        //15 ~ 19bit : 기준+3 열의 정보

                        //각 5bit 내에서...
                        //0 ~ 1bit : 해당 열의 뿌요의 수 : 1개 = 0, 4개 = 3
                        //2 ~ 4bit : 해당 열의 맨 아래에 있는 뿌요의 위치 : 기준 뿌요의 -2행 = 0, 기준 뿌요의 +2행 = 4

                        //가령 수평으로 쭉 이어져 있는 일자 모양은
                        //모든 열에서 뿌요의 수 1개, 맨 아래에 있는 뿌요의 위치는 기준 뿌요와 동일한 행이므로
                        //010_00-010_00-010_00-010_00이 된다.
                        int shape = m_PuyoShape[k];
                        int index = 0;
                        int nextLoc = 0;

                        //+3열의 적합성 확인
                        if (shape >> 15 > 0)
                        {
                            //뿌요가 7 이상의 열에 있거나 맨 아래 뿌요가 땅 밑에 있을 경우
                            //불가능한 모양이므로 다음 모양으로 넘어간다.
                            int num = shape >> 15;
                            int baseY = j + (num >> 2) - 2;
                            int addCount = (num & 3) + 1;
                            if (i >= 3 || baseY < 0)
                                continue;

                            //맨 아래 뿌요가 공중에 떠 있거나 해당 열에 올라갈 수 있는 뿌요의 수를 넘어선 경우
                            //불가능한 모양이므로 다음 모양으로 넘어간다.
                            int count = m_PuyoCount[i + 3];
                            if (baseY > count || count + addCount > maxCount)
                                continue;

                            //해당 열에 있는 뿌요를 다음 location에 저장한다.
                            for (int t = 0; t < addCount; t++)
                            {
                                int loc = (i + 3) << 4 | (baseY + t);
                                nextLoc |= loc << (index * 7);
                                index++;
                            }
                        }

                        //+2열의 적합성 확인
                        if (shape >> 10 > 0)
                        {
                            //뿌요가 7 이상의 열에 있거나 맨 아래 뿌요가 땅 밑에 있을 경우
                            //불가능한 모양이므로 다음 모양으로 넘어간다.
                            int num = (shape >> 10) & 31;
                            int baseY = j + (num >> 2) - 2;
                            int addCount = (num & 3) + 1;
                            if (i >= 4 || baseY < 0)
                                continue;

                            //맨 아래 뿌요가 공중에 떠 있거나 해당 열에 올라갈 수 있는 뿌요의 수를 넘어선 경우
                            //불가능한 모양이므로 다음 모양으로 넘어간다.
                            int count = m_PuyoCount[i + 2];
                            if (baseY > count || count + addCount > maxCount)
                                continue;

                            //해당 열에 있는 뿌요를 다음 location에 저장한다.
                            for (int t = 0; t < addCount; t++)
                            {
                                int loc = (i + 2) << 4 | (baseY + t);
                                nextLoc |= loc << (index * 7);
                                index++;
                            }
                        }

                        //+1열의 적합성 확인
                        if (shape >> 5 > 0)
                        {
                            //뿌요가 7 이상의 열에 있거나 맨 아래 뿌요가 땅 밑에 있을 경우
                            //불가능한 모양이므로 다음 모양으로 넘어간다.
                            int num = (shape >> 5) & 31;
                            int baseY = j + (num >> 2) - 2;
                            int addCount = (num & 3) + 1;
                            if (i >= 5 || baseY < 0)
                                continue;

                            //맨 아래 뿌요가 공중에 떠 있거나 해당 열에 올라갈 수 있는 뿌요의 수를 넘어선 경우
                            //불가능한 모양이므로 다음 모양으로 넘어간다.
                            int count = m_PuyoCount[i + 1];
                            if (baseY > count || count + addCount > maxCount)
                                continue;

                            //해당 열에 있는 뿌요를 다음 location에 저장한다.
                            for (int t = 0; t < addCount; t++)
                            {
                                int loc = (i + 1) << 4 | (baseY + t);
                                nextLoc |= loc << (index * 7);
                                index++;
                            }
                        }

                        //기준 열의 적합성 확인
                        int baseAdd = (shape & 3) + 1;
                        //해당 열에 올라갈 수 있는 뿌요의 수를 넘어서지 않은 경우
                        if (puyoCount + baseAdd <= maxCount)
                        {
                            //해당 열에 있는 뿌요를 다음 location에 저장한다.
                            for (int t = 0; t < baseAdd; t++)
                            {
                                int loc = i << 4 | (j + t);
                                nextLoc |= loc << (index * 7);
                                index++;
                            }

                            //만약 이번 뿌요를 추가해 목표 연쇄에 도달한다면
                            //실제 발화 가능한 위치에서 발화가 시작되는지 확인한다.
                            if (m_Rensa == DEST_RENSA - 1 && !IsLocationCorrect(nextLoc))
                                continue;

                            //아무 이상 없이 뿌요를 추가할 수 있다.
                            //네 가지 색을 모두 추가해 본다.
                            //m_Complete가 true가 되면 바로 프로그램이 종료되도록 한다.
                            FindRensa(nextLoc);
                            if (m_Complete) return;

                            FindRensa(nextLoc | (1 << 28));
                            if (m_Complete) return;

                            FindRensa(nextLoc | (2 << 28));
                            if (m_Complete) return;

                            FindRensa(nextLoc | (3 << 28));
                            if (m_Complete) return;
                        }
                    }
                }
            }

            //처음이 아니라면...
            if (location != 0)
            {
                //모든 경우의 수를 테스트했다.
                //연쇄 수를 1 줄이고 필드를 원래대로 되돌린다.
                m_Rensa--;
                RevertField(location);
            }
        }

        static void RevertField(int location)
        {
            //뿌요 필드에서 location에서 뽑아낸 위치의 뿌요를 없앤다.
            //각 열마다 올려진 뿌요의 수를 되돌린다.
            for (int i = 0; i < 4; i++)
            {
                int loc = (location >> (i * 7)) & 127;
                int x = loc >> 4;
                int y = loc & 15;

                m_PuyoField[x, y] = 0;
                m_PuyoCount[x]--;
            }

            //중력을 적용해 뿌요 필드를 원래대로 되돌린다.
            for (int i = 0; i < 6; i++)
            {
                int index = 0;
                for (int j = 0; j < 13; j++)
                {
                    if (m_PuyoField[i, j] == 0)
                        continue;
                    if (index != j)
                    {
                        m_PuyoField[i, index] = m_PuyoField[i, j];
                        m_PuyoField[i, j] = 0;
                    }
                    index++;
                }
            }
        }


        static bool IsLocationCorrect(int location)
        {
            //터지는 뿌요중에 2열 12행/3열 10행/3열 11행/4열 12행의 뿌요가 있는지 확인한다.
            //해당 위치의 뿌요가 하나라도 터지지 않으면 현실에선 불가능한 모양이다.
            for (int i = 0; i < 4; i++)
            {
                int loc = (location >> (i * 7)) & 127;
                if (loc == 27 || loc == 41 || loc == 42 || loc == 59)
                    return true;
            }

            return false;
        }

        static bool IsChainable()
        {
            //2열에는 13개의 뿌요가 채워져 있고, 1열에는 13개가 채워져 있지 않은 경우처럼
            //더 이상 사용할 수 없는 공간이 있는지 확인한다.
            if (m_PuyoCount[0] < 13 && m_PuyoCount[1] == 13)
                return false;
            if (m_PuyoCount[5] < 13 && m_PuyoCount[4] == 13)
                return false;
            if ((m_PuyoCount[5] < 13 || m_PuyoCount[4] < 13) && m_PuyoCount[3] == 13)
                return false;
            return true;
        }


        //버려진 코드
        /*
        static bool[,] m_FieldChecked;
        static int m_PoppedCount;

        static bool CheckRensa()
        {
            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < 12; j++)
                {
                    m_FieldChecked[i, j] = false;
                }
            }

            bool popped = false;

            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < 12; j++)
                {
                    if (m_FieldChecked[i, j])
                        continue;
                    if (m_PuyoField[i, j] == 0)
                        break;

                    m_PoppedCount = 0;
                    CheckPuyoPopped(i, j, m_PuyoField[i, j]);

                    if (m_PoppedCount > 4 || m_PoppedCount == 4 && popped)
                        return false;
                    if (m_PoppedCount == 4)
                        popped = true;
                }
            }

            return popped;
        }

        static void CheckPuyoPopped(int i, int j, byte color)
        {
            m_PoppedCount++;
            m_FieldChecked[i, j] = true;
            if (m_PoppedCount > 4) return;

            if (i > 0 && !m_FieldChecked[i - 1, j] && m_PuyoField[i - 1, j] == color)
            {
                CheckPuyoPopped(i - 1, j, color);
                if (m_PoppedCount > 4) return;
            }

            if (i < 5 && !m_FieldChecked[i + 1, j] && m_PuyoField[i + 1, j] == color)
            {
                CheckPuyoPopped(i + 1, j, color);
                if (m_PoppedCount > 4) return;
            }

            if (j > 0 && !m_FieldChecked[i, j - 1] && m_PuyoField[i, j - 1] == color)
            {
                CheckPuyoPopped(i, j - 1, color);
                if (m_PoppedCount > 4) return;
            }

            if (j < 11 && !m_FieldChecked[i, j + 1] && m_PuyoField[i, j + 1] == color)
                CheckPuyoPopped(i, j + 1, color);
        }
        */
    }
}
