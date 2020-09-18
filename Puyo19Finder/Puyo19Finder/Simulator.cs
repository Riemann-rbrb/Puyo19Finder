using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Puyo19Finder
{
    class Simulator
    {
        //임시 뿌요 필드
        byte[,] m_PuyoField;
        //필드 내의 해당 위치의 연결 체크가 끝났느냐
        bool[,] m_FieldChecked;

        //연결된 뿌요들의 위치
        int[] m_PoppedPuyo;
        //연결된 뿌요의 수
        int m_PoppedCount;

        public Simulator()
        {
            //변수 초기화
            m_PuyoField = new byte[6, 13];
            m_FieldChecked = new bool[6, 12];
            m_PoppedPuyo = new int[4];
        }


        //location이 0이면 발화 위치 체크를 하지 않는다.
        //location이 0이 아니면 발화 위치를 체크한다.
        public bool CheckRensa(byte[,] field, int rensa, int location)
        {
            //뿌요 필드를 복사하고, 연결 체크 배열을 초기화한다.
            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < 12; j++)
                {
                    m_FieldChecked[i, j] = false;
                    m_PuyoField[i, j] = field[i, j];
                }

                m_PuyoField[i, 12] = field[i, 12];
            }

            //뿌요가 터졌는지 여부
            bool popped = false;

            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < 12; j++) //6 * 12의 모든 위치에서...
                {
                    //해당 위치의 체크가 끝났다면 다음 위치로 넘어간다.
                    if (m_FieldChecked[i, j])
                        continue;
                    //해당 위치에 뿌요가 없다면 다음 위치로 넘어간다.
                    if (m_PuyoField[i, j] == 0)
                        break;

                    //연결된 뿌요의 수를 초기화하고, 해당 위치의 뿌요가 터지는지를 확인한다.
                    m_PoppedCount = 0;
                    CheckPuyoPopped(i, j, m_PuyoField[i, j]);

                    //만약 5개 이상의 뿌요가 터지거나 두 뭉치 이상의 뿌요가 터졌다면, 연쇄는 폭발한 것이다.
                    if (m_PoppedCount > 4 || m_PoppedCount == 4 && popped)
                        return false;
                    //4개의 뿌요가 터졌다면...
                    if (m_PoppedCount == 4)
                    {
                        //뿌요 터짐 여부를 true로 바꾼다.
                        popped = true;
                        for (int k = 0; k < 4; k++)
                        {
                            int loc = m_PoppedPuyo[k];
                            if (location > 0)
                            {
                                //발화 위치 체크가 필요할 경우,
                                //location과 터지는 뿌요의 위치가 같은지 확인한다.
                                //확인하지 않으면 모형의 중복이 발생한다.
                                int shouldPopped = (location >> (k * 7)) & 127;
                                if (!m_PoppedPuyo.Contains(shouldPopped))
                                    return false;
                            }

                            //터진 뿌요를 제거한다.
                            int x = loc >> 4;
                            int y = loc & 15;
                            m_PuyoField[x, y] = 0;
                        }
                    }
                }
            }

            //뿌요가 터지지 않은 경우, 연쇄는 잘못된 것이다.
            if (!popped)
                return false;
            //모든 뿌요가 터졌다면, 연쇄에 잘못된 부분이 없는 것이다.
            if (rensa == 1)
                return true;

            //중력을 적용한다.
            ApplyGravity();
            //두 번째부터는 아래 메소드를 사용한다.
            return CheckRensa(rensa - 1);
        }

        private bool CheckRensa(int rensa)
        {
            //연결 체크 배열을 초기화한다.
            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < 12; j++)
                {
                    m_FieldChecked[i, j] = false;
                }
            }

            //뿌요가 터졌는지 여부
            bool popped = false;

            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < 12; j++) //6 * 12의 모든 위치에서...
                {
                    //해당 위치의 체크가 끝났다면 다음 위치로 넘어간다.
                    if (m_FieldChecked[i, j])
                        continue;
                    //해당 위치에 뿌요가 없다면 다음 위치로 넘어간다.
                    if (m_PuyoField[i, j] == 0)
                        break;

                    //연결된 뿌요의 수를 초기화하고, 해당 위치의 뿌요가 터지는지를 확인한다.
                    m_PoppedCount = 0;
                    CheckPuyoPopped(i, j, m_PuyoField[i, j]);

                    //만약 5개 이상의 뿌요가 터지거나 두 뭉치 이상의 뿌요가 터졌다면, 연쇄는 폭발한 것이다.
                    if (m_PoppedCount > 4 || m_PoppedCount == 4 && popped)
                        return false;
                    //4개의 뿌요가 터졌다면...
                    if (m_PoppedCount == 4)
                    {
                        //뿌요 터짐 여부를 true로 바꾼다.
                        popped = true;
                        for (int k = 0; k < 4; k++)
                        {
                            //터진 뿌요를 제거한다.
                            int loc = m_PoppedPuyo[k];
                            int x = loc >> 4;
                            int y = loc & 15;
                            m_PuyoField[x, y] = 0;
                        }
                    }
                }
            }

            //뿌요가 터지지 않은 경우, 연쇄는 잘못된 것이다.
            if (!popped)
                return false;
            //모든 뿌요가 터졌다면, 연쇄에 잘못된 부분이 없는 것이다.
            if (rensa == 1)
                return true;

            //중력을 적용한다.
            ApplyGravity();
            //테스트를 반복한다.
            return CheckRensa(rensa - 1);
        }


        private void CheckPuyoPopped(int i, int j, byte color)
        {
            //연결된 뿌요의 수를 1 올린다.
            //해당 위치의 체크 여부를 true로 바꾼다.
            m_PoppedCount++;
            m_FieldChecked[i, j] = true;

            //연결된 뿌요의 수가 5 이상이라면 체크를 끝낸다.
            if (m_PoppedCount > 4) return;
            //연결된 뿌요 위치 배열에 위치를 추가한다.
            m_PoppedPuyo[m_PoppedCount - 1] = i << 4 | j;

            if (i > 0 && !m_FieldChecked[i - 1, j] && m_PuyoField[i - 1, j] == color)
            {
                //만약 왼쪽을 체크하지 않았고, 왼쪽에 있는 뿌요와 색깔이 같다면
                //위치를 왼쪽으로 옮겨 체크를 반복한다.
                CheckPuyoPopped(i - 1, j, color);
                //연결된 뿌요의 수가 5 이상이라면 체크를 끝낸다.
                if (m_PoppedCount > 4) return;
            }

            if (i < 5 && !m_FieldChecked[i + 1, j] && m_PuyoField[i + 1, j] == color)
            {
                //만약 오른쪽을 체크하지 않았고, 오른쪽에 있는 뿌요와 색깔이 같다면
                //위치를 오른쪽으로 옮겨 체크를 반복한다.
                CheckPuyoPopped(i + 1, j, color);
                //연결된 뿌요의 수가 5 이상이라면 체크를 끝낸다.
                if (m_PoppedCount > 4) return;
            }

            if (j > 0 && !m_FieldChecked[i, j - 1] && m_PuyoField[i, j - 1] == color)
            {
                //만약 아래를 체크하지 않았고, 아래에 있는 뿌요와 색깔이 같다면
                //위치를 아래로 옮겨 체크를 반복한다.
                CheckPuyoPopped(i, j - 1, color);
                //연결된 뿌요의 수가 5 이상이라면 체크를 끝낸다.
                if (m_PoppedCount > 4) return;
            }

            //만약 위를 체크하지 않았고, 위에 있는 뿌요와 색깔이 같다면
            //위치를 위로 옮겨 체크를 반복한다.
            if (j < 11 && !m_FieldChecked[i, j + 1] && m_PuyoField[i, j + 1] == color)
                CheckPuyoPopped(i, j + 1, color);
        }

        private void ApplyGravity()
        {
            //뿌요 필드에 중력을 적용한다.
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
    }
}
