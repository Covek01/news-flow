import User from "../models/User";

interface Session {
    id: string;
    expires: string;
  }

  export const lsGetSession: () => Session | null = () => {
    const lssession = localStorage.getItem("session");
    if (!lssession) return null;
    return JSON.parse(lssession);
  };
  
  export const lsSetSession = (session: Session) =>
    localStorage.setItem("session", JSON.stringify(session));
  
  export const lsRemoveSession = () => localStorage.removeItem("session");
  
  export const lsGetUser = () => JSON.parse(localStorage.getItem("user") || "{}");
  
  export const lsSetUser = (user: User) =>
    localStorage.setItem("user", JSON.stringify(user));
  
  export const lsRemoveUser = () => localStorage.removeItem("user");
  