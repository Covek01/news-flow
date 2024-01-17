import { ReactElement } from "react";
import { Navigate } from "react-router-dom";
import { useAuthContext } from "../../contexts/auth.context";

const NotAuthenticatedGuard = ({ children }: { children: ReactElement }) => {
  const { isAuthenticated } = useAuthContext();

  if (! isAuthenticated()) {
    return <Navigate to="/signin"></Navigate>;
  }

  return <>{children}</>;
};

export default NotAuthenticatedGuard;
