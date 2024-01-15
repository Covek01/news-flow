import { ReactElement } from "react";
import { Navigate } from "react-router-dom";
import { useAuthContext } from "../../contexts/auth.context";

const AuthenticatedGuard = ({ children }: { children: ReactElement }) => {
  const { isAuthenticated } = useAuthContext();

  if (isAuthenticated()) {
    return <Navigate to="/home"></Navigate>;
  }

  return <>{children}</>;
};

export default AuthenticatedGuard;
