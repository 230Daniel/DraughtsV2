import React from "react";
import { BrowserRouter as Router, Switch, Route } from "react-router-dom";
import Footer from "./components/layout/Footer";
import Navbar from "./components/layout/Navbar";
import Index from "./pages/Index";

export default class App extends React.Component{
	render(){
		return(
			<Router>
				<main>
					<Navbar/>
					<Switch>
						<Route path="/" component={Index}/>
					</Switch>
				</main>
				<footer>
					<Footer/>
				</footer>
			</Router>
		)
	}
}
