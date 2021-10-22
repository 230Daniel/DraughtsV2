import React from "react";
import Piece from "./Piece";

import styles from "./Tile.Module.css";

export default class Tile extends React.Component {
	render() {
		return (
			<div className={this.getClassNames()} onClick={() => this.props.onClick(this.props.coords)}>
				{this.props.playable &&
					<Piece
						piece={this.props.tile}
						selected={this.props.selected} />
				}
			</div>
		)
	}

	getClassNames() {
		if (this.props.playable) {
			var classNames = [ styles.tile, styles.white ];
			if (this.props.selectable) classNames.push(styles.selectable);
			if (this.props.selected) classNames.push(styles.selected);
			return classNames.join(" ");
		} else {
			return `${styles.tile} ${styles.black}`;
		}
	}
}
