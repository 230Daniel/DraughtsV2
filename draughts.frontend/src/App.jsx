import React from "react";
import { BrowserRouter as Router, Switch, Route } from "react-router-dom";
import Footer from "./components/layout/Footer";
import Navbar from "./components/layout/Navbar";

export default class App extends React.Component{
	render(){
		return(
			<Router>
				<main>
					<Navbar/>
					<Switch>
					</Switch>
				</main>
				<footer>
					<Footer/>
				</footer>
			</Router>
		)
	}
}
