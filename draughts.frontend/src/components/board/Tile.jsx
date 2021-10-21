import React from "react";

import styles from "./Tile.Module.css";

export default class Tile extends React.Component{
	constructor(props){
		super(props);
	}

	render(){
		return(
			<div className={styles.tile}>
				<span>({this.props.x}, {this.props.y})</span>
			</div>
		)
	}
}