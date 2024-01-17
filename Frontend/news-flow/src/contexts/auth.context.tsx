import {
    createContext,
    ReactNode,
    useContext,
    useEffect,
    useState,
  } from "react";
  import { api } from "../services/Service"
  import User from "../models/User";
  import {
    lsGetSession,
    lsGetUser,
    lsRemoveSession,
    lsRemoveUser,
    lsSetSession,
    lsSetUser,
  } from "../utils/helpers";
  
  interface AuthState {
    user: User;
    session: {
      id: string;
      expires: string;
    };
  }
  
  interface AuthStateContextI {
    user: User | null;
    signin: (authState: AuthState) => void;
    signout: () => void;
    autoLogin: () => void;
    isAuthenticated: () => boolean;
    userIsCompany: () => boolean;
  }
  
  interface AuthStateProviderProps {
    children: ReactNode;
  }
  
  const AuthStateContext = createContext<AuthStateContextI>(
    {} as AuthStateContextI
  );
  
  export function useAuthContext() {
    const context = useContext(AuthStateContext);
    if (!context)
      throw new Error("useAuthState must be used within AuthProvider");
  
    return context;
  }
  
  export function AuthStateProvider({ children }: AuthStateProviderProps) {
    const [authState, setAuthState] = useState<AuthState | null>(null);
  
    useEffect(() => {
      autoLogin();
    }, []);
  
    const signin = (authState: AuthState) => {
      if (!authState.user || !authState.session) return;
  
      setAuthState(authState);
      lsSetSession(authState.session);
      lsSetUser(authState.user);
    };
  
    const signout = () => {
      if (lsGetSession() == null) {
        return;
      }
  
      setAuthState(null);
  
      api.put("/user/signout", {}).finally(() => {
        lsRemoveSession();
        lsRemoveUser();
      });
    };
  
    const autoLogin = () => {
      const user = lsGetUser();
      const session = lsGetSession();
  
      if (!user || !session) {
        return;
      }
  
      setAuthState({ user, session });
    };
  
    const isAuthenticated: () => boolean = () => {
      if (!authState?.session) return true;
  
      if (new Date(authState.session.expires) < new Date()) {
        signout();
        return false;
      }
  
      return true;
    };
  
    const userIsCompany = () => {
      return authState?.user?.role === "Company";
    };
  
    return (
      <AuthStateContext.Provider
        value={{
          autoLogin,
          signin,
          signout,
          isAuthenticated,
          user: authState?.user!,
          userIsCompany,
        }}
      >
        {children}
      </AuthStateContext.Provider>
    );
  }
  