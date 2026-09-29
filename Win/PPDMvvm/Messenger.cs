using System;
using System.Collections.Generic;
using System.Linq;

namespace PPDMvvm
{
    /// <summary>
    /// ビューモデルからビューへメッセージを送る仕組みです。
    /// 受信側は、不要になったら Unregister で登録を解除してください（受信者への参照を保持します）。
    /// </summary>
    public class Messenger
    {
        private readonly Dictionary<Type, List<Registration>> registrations = new Dictionary<Type, List<Registration>>();

        /// <summary>
        /// 既定のインスタンスを取得します。
        /// </summary>
        public static Messenger Default { get; } = new Messenger();

        /// <summary>
        /// メッセージの受信を登録します。
        /// </summary>
        /// <typeparam name="TMessage">メッセージの型。</typeparam>
        /// <param name="recipient">受信者。</param>
        /// <param name="action">受信したときの処理。</param>
        public void Register<TMessage>(object recipient, Action<TMessage> action)
        {
            lock (registrations)
            {
                if (!registrations.TryGetValue(typeof(TMessage), out List<Registration> list))
                {
                    list = new List<Registration>();
                    registrations.Add(typeof(TMessage), list);
                }
                list.Add(new Registration(recipient, action));
            }
        }

        /// <summary>
        /// メッセージの受信の登録を解除します。
        /// </summary>
        /// <typeparam name="TMessage">メッセージの型。</typeparam>
        /// <param name="recipient">受信者。</param>
        /// <param name="action">登録した処理。</param>
        public void Unregister<TMessage>(object recipient, Action<TMessage> action)
        {
            lock (registrations)
            {
                if (registrations.TryGetValue(typeof(TMessage), out List<Registration> list))
                {
                    list.RemoveAll(r => r.Recipient == recipient && r.Action.Equals(action));
                }
            }
        }

        /// <summary>
        /// 受信者の登録をすべて解除します。
        /// </summary>
        /// <param name="recipient">受信者。</param>
        public void Unregister(object recipient)
        {
            lock (registrations)
            {
                foreach (var list in registrations.Values)
                {
                    list.RemoveAll(r => r.Recipient == recipient);
                }
            }
        }

        /// <summary>
        /// メッセージを送ります。TMessage 型で登録したすべての受信者が受け取ります。
        /// </summary>
        /// <typeparam name="TMessage">メッセージの型。</typeparam>
        /// <param name="message">メッセージ。</param>
        public void Send<TMessage>(TMessage message)
        {
            Registration[] targets;
            lock (registrations)
            {
                if (!registrations.TryGetValue(typeof(TMessage), out List<Registration> list))
                {
                    return;
                }
                targets = list.ToArray();
            }
            foreach (var registration in targets)
            {
                ((Action<TMessage>)registration.Action)(message);
            }
        }

        private class Registration
        {
            public object Recipient { get; }

            public Delegate Action { get; }

            public Registration(object recipient, Delegate action)
            {
                Recipient = recipient;
                Action = action;
            }
        }
    }
}
