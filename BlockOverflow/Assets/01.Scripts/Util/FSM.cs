/// <summary>
/// Finite State Machine
/// </summary>
/// <typeparam name="T">Parent Type of FSM. States can access their owner instance with this</typeparam>
public class FSM<T>
{
    public T Owner;
    public IState<T> State;
    public IState<T> NextState { get; private set; }

    public FSM(T owner)
    {
        Owner = owner;
        State = null;
        NextState = null;
    }

    /// <summary>
    /// Set the next state with type R. new instance of R will be automatically created
    /// </summary>
    /// <typeparam name="R"></typeparam>
    /// <returns></returns>
    ///
    /// 출처가 T에서 나온 상태인게 맞냐??
    /// R은 사실 State 여러개 중 한개일거임 예시로는 R <IdleState T<PlayerState>>
    public FSM<T> Set<R>() where R : IState<T>, new()
    {
        NextState = new R();
        if (NextState is State<T> state)
        {
            state.Owner = this;
        }
        return this;
    }

    /// <summary>
    /// Set the next state with an existing IState<T> object.
    /// </summary>
    /// <param name="state">new state to set</param>
    /// <returns></returns>
    public FSM<T> Set(IState<T> state)
    {
        NextState = state;
        if (NextState is State<T> stateT)
        {
            stateT.Owner = this;
        }
        return this;
    }

    /// <summary>
    /// Update the FSM. use it in MonoBehaviour.Update()
    /// </summary>
    /// <returns></returns>
    public FSM<T> Update()
    {
        if (NextState != State)
        {
            State?.OnEnd(Owner);
            State = NextState;
            State?.OnBegin(Owner);
        }
        State?.OnUpdate(Owner);
        return this;
    }
}

public interface IState<T>
{
    public void OnBegin(T owner);
    public void OnUpdate(T owner);
    public void OnEnd(T owner);
}

/// <summary>
/// Root class for States.
/// </summary>
/// <typeparam name="T">matches with its FSM's T</typeparam>
public abstract class State<T> : IState<T>
{
    public FSM<T> Owner;

    /// <summary>
    /// Calls its owner FSM's Set<R>()
    /// </summary>
    /// <typeparam name="R">equals with FSM<T>.Set<R>()'s R</typeparam>
    public void Set<R>() where R : IState<T>, new()
    {
        Owner.Set<R>();
    }

    public virtual void OnBegin(T owner) { }
    public virtual void OnUpdate(T owner) { }
    public virtual void OnEnd(T owner) { }
}