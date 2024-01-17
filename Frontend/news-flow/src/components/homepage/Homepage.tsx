import Bar from "../bar/Bar"
import * as React from 'react';
import NewsContainer from "./NewsContainer";
import News from "../../models/News"


interface HomepageProps{
    newsList: News[]
}

const Homepage: React.FC = () => {
    const [eff, setEff] = React.useState(false)
    return (
        <div>
            <Bar />
            <NewsContainer newsList={[]}/>
        </div>
    );
}

export default Homepage;