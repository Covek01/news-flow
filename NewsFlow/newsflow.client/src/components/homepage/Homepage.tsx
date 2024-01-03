import Bar from "../bar/Bar"
import * as React from 'react';



const Homepage = () => {
    const [eff, setEff] = React.useState(false)
    return (
        <>
            <Bar />
        </>
    );
}

export default Homepage;