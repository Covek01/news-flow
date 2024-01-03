import Bar from "../bar/Bar"
import * as React from 'react';
import NewsContainer from "./NewsContainer";



const Homepage = () => {
    const [eff, setEff] = React.useState(false)
    return (
        <div>
            <Bar />
            <NewsContainer />
        </div>
    );
}

export default Homepage;