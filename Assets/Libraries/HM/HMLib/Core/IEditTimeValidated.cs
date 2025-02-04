public interface IEditTimeValidated {

#if UNITY_EDITOR
    bool __Validate();
#endif
}
