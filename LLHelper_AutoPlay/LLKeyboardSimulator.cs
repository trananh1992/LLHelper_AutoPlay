﻿using System;
using System.Collections.Generic;
using System.Threading;

namespace LLHelper_AutoPlay
{
    public class LLKeyboardSimulator
    {
        public delegate void OnOver();
        public OnOver onOver;

        private MusicNotes mn;

        private Dictionary<byte, byte> pos2key;

        private bool isRun = false;
        private long startTick;
        private int trimValue = 150000;

        private void TimeLine(object obj)
        {
            Notes_list[] nl = obj as Notes_list[];
            for (int i = 0; i < nl.Length; i++)
            {
                nl[i].notes_attribute = 1;
            }
            startTick = DateTime.Now.Ticks;
            int t = 0;
            while (isRun)
            {
                for (int i = t; i < nl.Length; i++)
                {
                    if (nl[i].notes_attribute == 1)
                    {
                        if (nl[i].timing_sec <= (DateTime.Now.Ticks - startTick) / 10000000.0f)
                        {
                            startTick += 74;
                            nl[i].notes_attribute = 0;
                            t++;
                            new Thread(SendForATime).Start(nl[i]);
                            
                        }

                        break;
                    }
                }
                if (nl[nl.Length - 1].notes_attribute == 0)
                {
                    //textBox3.Text = "结束\r\n\r\n" + textBox3.Text;
                    //mn = null;
                    break;
                }

                Thread.Sleep(1);
            }
            isRun = false;
            onOver?.Invoke();
        }

        private void SendForATime(object obj)
        {
            Notes_list nl = obj as Notes_list;
            float lastTime = nl.effect == 3 || nl.effect == 13 ? nl.effect_value : 0.03f;

            byte pos = nl.position;

            long last = DateTime.Now.Ticks;
            do
            {
                Thread.Sleep(1);
                KeyDown(pos);
            }
            while (isRun && (DateTime.Now.Ticks - last) <= lastTime * 10000000f);

            KeyUp(pos);
            KeyUp(pos);
            KeyUp(pos);
            KeyUp(pos);
        }

        private void KeyDown(byte pos)
        {
            Win32API.keybd_event(pos2key[pos], 0, 0, 0);
        }

        private void KeyUp(byte pos)
        {
            Win32API.keybd_event(pos2key[pos], 0, 2, 0);
        }

        public void Init(Dictionary<byte, byte> pos2key1)
        {
            pos2key = new Dictionary<byte, byte>();
            pos2key.Add(1, Win32API.Key32.Key_L);
            pos2key.Add(2, Win32API.Key32.Key_K);
            pos2key.Add(3, Win32API.Key32.Key_J);
            pos2key.Add(4, Win32API.Key32.Key_H);
            pos2key.Add(5, Win32API.Key32.Key_Space);
            pos2key.Add(6, Win32API.Key32.Key_F);
            pos2key.Add(7, Win32API.Key32.Key_D);
            pos2key.Add(8, Win32API.Key32.Key_S);
            pos2key.Add(9, Win32API.Key32.Key_A);
        }

        public void Reset(MusicNotes mn)
        {
            this.mn = mn;
            isRun = false;
        }

        public void Start()
        {
            IntPtr h = Win32API.FindWindow(null, "Bluestacks App Player");
            Win32API.SetForegroundWindow(h);
            Notes_list[] notelist = new List<Notes_list>(mn.response_data.live_list[0].live_info.notes_list).ToArray();
            float timeStart = notelist[0].timing_sec;

            for (int i = 0; i < notelist.Length; i++)
            {
                notelist[i].timing_sec -= timeStart;
            }

            isRun = true;
            new Thread(TimeLine).Start(notelist);
        }

        public void Stop()
        {
            isRun = false;
            onOver?.Invoke();
        }

        public void TrimForward()
        {
            startTick -= trimValue;
        }

        public void TrimBackward()
        {
            startTick += trimValue;
        }

    }
}